#!/usr/bin/env python3
"""Install, update, back up, restore, or uninstall UzonMail on Linux."""

from __future__ import annotations

import argparse
import dataclasses
import datetime as dt
import getpass
import hashlib
import json
import os
import pathlib
import platform
import re
import secrets
import shlex
import shutil
import stat
import struct
import subprocess
import sys
import tempfile
import time
import urllib.error
import urllib.parse
import urllib.request
import zipfile
from collections.abc import Iterable, Sequence
from typing import Any

APPLICATION_NAME = "UzonMail"
SERVICE_NAME = "uzon-mail.service"
SERVICE_USER = "uzonmail"
INSTALL_ROOT = pathlib.Path("/var/www/uzonmail")
SERVICE_HOME = INSTALL_ROOT / "data"
INSTALLER_STATE_ROOT = pathlib.Path("/var/lib/uzonmail-installer")
INSTALL_STATE_PATH = INSTALLER_STATE_ROOT / "install-state.json"
PENDING_CONFIG_PATH = INSTALLER_STATE_ROOT / "pending-config.json"
SERVICE_UNIT_PATH = pathlib.Path("/etc/systemd/system") / SERVICE_NAME
LOCK_PATH = pathlib.Path("/run/lock/uzonmail-installer.lock")
TEMP_ROOT: pathlib.Path | None = None
DEFAULT_BACKUP_ROOT = pathlib.Path("/var/backups/uzonmail")
LATEST_MANIFEST_URL = "https://uzonmail.uzoncloud.com/updates/latest.json"
DOTNET_INSTALL_URL = "https://dot.net/v1/dotnet-install.sh"
DOTNET_INSTALL_SIGNATURE_URL = "https://dot.net/v1/dotnet-install.sig"
DOTNET_INSTALL_KEY_URL = "https://dot.net/v1/dotnet-install.asc"
DOTNET_INSTALL_KEY_FINGERPRINT = "2B930AB1228D11D5D7F6B6ACB9CF1A51FC7D3ACF"
DOTNET_INSTALL_ROOT = pathlib.Path("/usr/share/dotnet")
MANAGED_DOTNET_PATH = DOTNET_INSTALL_ROOT / "dotnet"
PACKAGE_DIRECTORY_NAME = "service-linux-x64"
SERVICE_ASSEMBLY_NAME = "UzonMailService.dll"
PRODUCTION_CONFIG_NAME = "appsettings.Production.json"
BACKUP_MANIFEST_NAME = "backup-manifest.json"
INSTALL_STATE_SCHEMA_VERSION = 1
BACKUP_SCHEMA_VERSION = 1
NETWORK_TIMEOUT_SECONDS = 30
MAX_MANIFEST_BYTES = 1024 * 1024
MAX_PACKAGE_BYTES = 2 * 1024 * 1024 * 1024
MAX_ASSEMBLY_BYTES = 256 * 1024 * 1024
MAX_ARCHIVE_ENTRIES = 100_000
MAX_EXTRACTED_BYTES = 4 * 1024 * 1024 * 1024
DEFAULT_BASE_URL = "http://localhost:22345"
DEFAULT_ADMIN_USER = "admin"
DEFAULT_TOKEN_EXPIRATION_MILLISECONDS = 24 * 60 * 60 * 1000
SUPPORTED_FRAMEWORKS = {
    "Microsoft.AspNetCore.App": "aspnetcore",
    "Microsoft.NETCore.App": "dotnet",
}
LINUX_RUNTIME_IDENTIFIER = "linux-x64"
SHA256_PATTERN = re.compile(r"^[0-9a-f]{64}$")
PERSISTENT_NAMES = (PRODUCTION_CONFIG_NAME, "data")
WRITABLE_DIRECTORY_NAMES = ("data", "logs")


class InstallerError(RuntimeError):
    """Describe an expected installer failure with actionable context."""


class OperationCancelled(InstallerError):
    """Indicate that the user declined a requested system change."""


@dataclasses.dataclass(frozen=True, order=True)
class Version:
    """Represent a normalized four-part UzonMail or .NET version."""

    major: int
    minor: int
    build: int
    revision: int

    @classmethod
    def parse(cls, value: str, field_name: str = "version") -> Version:
        """Parse a two-to-four-part numeric version from an untrusted source."""
        parts = value.strip().split(".")
        if not 2 <= len(parts) <= 4 or any(
            not part.isdigit() or len(part) > 9 for part in parts
        ):
            raise InstallerError(f"Invalid {field_name}: {value!r}")
        normalized = [int(part) for part in parts]
        normalized.extend([0] * (4 - len(normalized)))
        return cls(*normalized)

    def __str__(self) -> str:
        return f"{self.major}.{self.minor}.{self.build}.{self.revision}"

    def to_runtime_string(self) -> str:
        """Format a .NET runtime version without an artificial zero revision."""
        version = f"{self.major}.{self.minor}.{self.build}"
        return f"{version}.{self.revision}" if self.revision else version


@dataclasses.dataclass(frozen=True)
class UpdateManifest:
    """Contain the validated update metadata required by the Linux installer."""

    version: Version
    environment: dict[str, Version]
    package_url: str
    package_sha256: str
    minimum_compatible_version: Version | None


@dataclasses.dataclass(frozen=True)
class InstallConfig:
    """Contain validated user input and generated installation secrets."""

    base_url: str
    admin_user: str
    admin_password: str
    encryption_key: str
    encryption_iv: str
    token_secret: str

    def to_json_value(self) -> dict[str, str]:
        """Serialize cached installer input without exposing it on the console."""
        return {
            "baseUrl": self.base_url,
            "adminUser": self.admin_user,
            "adminPassword": self.admin_password,
            "encryptionKey": self.encryption_key,
            "encryptionIv": self.encryption_iv,
            "tokenSecret": self.token_secret,
        }


@dataclasses.dataclass(frozen=True)
class InstallState:
    """Record resources owned by one completed UzonMail installation."""

    version: Version
    minimum_compatible_version: Version | None
    dotnet_path: pathlib.Path
    installed_at: dt.datetime

    def to_json_value(self) -> dict[str, Any]:
        """Serialize the authoritative installer state."""
        return {
            "schemaVersion": INSTALL_STATE_SCHEMA_VERSION,
            "version": str(self.version),
            "minCompatibleVersion": (
                str(self.minimum_compatible_version)
                if self.minimum_compatible_version is not None
                else None
            ),
            "dotnetPath": os.fspath(self.dotnet_path),
            "installedAt": self.installed_at.isoformat(),
        }


@dataclasses.dataclass(frozen=True)
class BackupManifest:
    """Describe validated persistent content in one backup directory."""

    version: Version
    created_at: dt.datetime
    contents: tuple[str, ...]


class CommandRunner:
    """Execute commands after the installer has established root authority."""

    def __init__(self, quiet: bool) -> None:
        self.quiet = quiet

    def run(
        self,
        arguments: Sequence[str | os.PathLike[str]],
        *,
        description: str,
        check: bool = True,
        capture_output: bool = False,
    ) -> subprocess.CompletedProcess[str]:
        """Run one command and preserve useful failure context."""
        command = [os.fspath(argument) for argument in arguments]
        print(f"-> {description}")
        print(f"   {shlex.join(command)}")
        try:
            return subprocess.run(
                command,
                check=check,
                text=True,
                capture_output=capture_output,
            )
        except FileNotFoundError as exception:
            raise InstallerError(
                f"Required command was not found: {command[0]}"
            ) from exception
        except subprocess.CalledProcessError as exception:
            details = (exception.stderr or exception.stdout or "").strip()
            suffix = f": {details}" if details else ""
            raise InstallerError(
                f"Command failed ({description}){suffix}"
            ) from exception


def confirm_change(description: str, quiet: bool, *, default: bool = False) -> None:
    """Require an explicit confirmation unless quiet mode is active."""
    if quiet:
        return
    suffix = "Y/n" if default else "y/N"
    try:
        response = input(f"{description} [{suffix}]: ").strip().lower()
    except EOFError as exception:
        raise InstallerError(
            "Interactive input is unavailable; use --quiet for automation."
        ) from exception
    accepted = response in {"y", "yes"} or (default and response == "")
    if not accepted:
        raise OperationCancelled("Operation cancelled by the user.")


def ask_yes_no(prompt: str, *, default: bool) -> bool:
    """Read a yes/no choice with a visible default."""
    suffix = "Y/n" if default else "y/N"
    while True:
        try:
            response = input(f"{prompt} [{suffix}]: ").strip().lower()
        except EOFError as exception:
            raise InstallerError(
                "Interactive input is unavailable; use --quiet for automation."
            ) from exception
        if response == "":
            return default
        if response in {"y", "yes"}:
            return True
        if response in {"n", "no"}:
            return False
        print("Please answer yes or no.")


def print_startup_notice(action: str, quiet: bool) -> None:
    """Explain the installer purpose and all persistent system changes."""
    print(f"{APPLICATION_NAME} Linux installer - {action}")
    print("This tool can download UzonMail and the required .NET runtimes over HTTPS.")
    print(f"It manages {INSTALL_ROOT} and {INSTALLER_STATE_ROOT}.")
    print(
        f"It manages system account '{SERVICE_USER}' and systemd unit {SERVICE_UNIT_PATH}."
    )
    print(
        f"Backups are preserved under {DEFAULT_BACKUP_ROOT} unless another path is selected."
    )
    print(
        "It does not change firewall rules or remove shared .NET runtimes during uninstall."
    )
    if quiet:
        print(
            "Quiet mode is active: defaults are used and change confirmations are skipped."
        )
    print()


def validate_platform() -> None:
    """Reject unsupported operating systems, architectures, and init systems."""
    if not sys.platform.startswith("linux"):
        raise InstallerError("This operation is supported only on Linux.")
    architecture = platform.machine().lower()
    if architecture not in {"x86_64", "amd64"}:
        raise InstallerError(
            f"Unsupported architecture: {architecture}. Only x64 is supported."
        )
    if (
        shutil.which("systemctl") is None
        or not pathlib.Path("/run/systemd/system").exists()
    ):
        raise InstallerError("A running systemd environment is required.")


def reexecute_as_root(arguments: Sequence[str], quiet: bool) -> None:
    """Replace the current process with one root-owned installer execution."""
    if getattr(os, "geteuid", lambda: -1)() == 0:
        return
    if shutil.which("sudo") is None:
        raise InstallerError(
            "sudo is required. Install sudo or run this script as root."
        )
    sudo_arguments = ["sudo"]
    if quiet:
        sudo_arguments.append("-n")
    sudo_arguments.extend(["python3", os.path.abspath(__file__), *arguments])
    try:
        os.execvp("sudo", sudo_arguments)
    except OSError as exception:
        raise InstallerError(
            f"Unable to restart the installer with sudo: {exception}"
        ) from exception


def validate_managed_path_layout() -> None:
    """Reject redirection of installer-owned paths through symbolic links."""
    managed_paths = (
        INSTALL_ROOT,
        INSTALLER_STATE_ROOT,
        SERVICE_UNIT_PATH,
        DOTNET_INSTALL_ROOT,
    )
    for managed_path in managed_paths:
        reject_existing_symlink_components(
            managed_path.absolute(), "Installer-managed path"
        )


def normalize_base_url(value: str) -> str:
    """Validate and normalize a public HTTP base URL."""
    parsed = urllib.parse.urlsplit(value.strip())
    if (
        parsed.scheme not in {"http", "https"}
        or not parsed.hostname
        or parsed.username
        or parsed.password
        or parsed.query
        or parsed.fragment
    ):
        raise InstallerError(
            "BaseUrl must be an HTTP(S) URL without credentials, query, or fragment."
        )
    return urllib.parse.urlunsplit(
        (parsed.scheme, parsed.netloc, parsed.path.rstrip("/"), "", "")
    )


def get_url_origin(value: str) -> str:
    """Return the scheme and authority used by ASP.NET CORS origin matching."""
    parsed = urllib.parse.urlsplit(normalize_base_url(value))
    return urllib.parse.urlunsplit((parsed.scheme, parsed.netloc, "", "", ""))


def create_private_temp_root() -> pathlib.Path:
    """Create one root-only temporary workspace for this invocation."""
    try:
        path = pathlib.Path(tempfile.mkdtemp(prefix="uzonmail-installer-"))
        path.chmod(0o700)
        return path
    except OSError as exception:
        raise InstallerError(
            f"Unable to create a private temporary directory: {exception}"
        ) from exception


def require_temp_root() -> pathlib.Path:
    """Return the initialized per-run temporary directory."""
    if TEMP_ROOT is None:
        raise InstallerError("The installer temporary directory is not initialized.")
    return TEMP_ROOT


def acquire_installer_lock() -> int:
    """Acquire the system-wide installer transaction lock."""
    import fcntl

    descriptor = -1
    try:
        descriptor = os.open(
            LOCK_PATH,
            os.O_CREAT | os.O_RDWR | os.O_NOFOLLOW,
            0o600,
        )
        os.fchmod(descriptor, 0o600)
        fcntl.flock(descriptor, fcntl.LOCK_EX | fcntl.LOCK_NB)
        return descriptor
    except BlockingIOError as exception:
        if descriptor >= 0:
            os.close(descriptor)
        raise InstallerError(
            "Another UzonMail installer operation is already running."
        ) from exception
    except OSError as exception:
        if descriptor >= 0:
            os.close(descriptor)
        raise InstallerError(
            f"Unable to acquire installer lock {LOCK_PATH}: {exception}"
        ) from exception


def ensure_private_temp_directory(path: pathlib.Path) -> None:
    """Create a subdirectory without leaving the current private workspace."""
    temp_root = require_temp_root()
    try:
        relative_parts = path.relative_to(temp_root).parts
    except ValueError as exception:
        raise InstallerError(
            f"Temporary path is outside {temp_root}: {path}"
        ) from exception
    current = temp_root
    for path_part in relative_parts:
        current /= path_part
        try:
            current_status = current.lstat()
            if not stat.S_ISDIR(current_status.st_mode):
                raise InstallerError(
                    f"Temporary path component is a link or non-directory: {current}"
                )
        except FileNotFoundError:
            try:
                current.mkdir(mode=0o700)
            except OSError as exception:
                raise InstallerError(
                    f"Unable to create temporary directory {current}: {exception}"
                ) from exception


def read_limited_file(
    path: pathlib.Path, maximum_bytes: int, description: str
) -> bytes:
    """Read an untrusted file only after enforcing a fixed size boundary."""
    try:
        file_status = path.lstat()
        if not stat.S_ISREG(file_status.st_mode):
            raise InstallerError(f"{description} is not a regular file: {path}")
        if file_status.st_size > maximum_bytes:
            raise InstallerError(
                f"{description} exceeds the {maximum_bytes}-byte limit: {path}"
            )
        return path.read_bytes()
    except OSError as exception:
        raise InstallerError(
            f"Unable to read {description} {path}: {exception}"
        ) from exception


def path_lexists(path: pathlib.Path) -> bool:
    """Return whether a path entry exists without following its final component."""
    try:
        path.lstat()
        return True
    except FileNotFoundError:
        return False
    except OSError as exception:
        raise InstallerError(
            f"Unable to inspect path {path}: {exception}"
        ) from exception


def atomic_write_file(path: pathlib.Path, content: bytes, mode: int) -> pathlib.Path:
    """Atomically write a file without following a pre-existing destination link."""
    try:
        path.relative_to(require_temp_root())
    except ValueError:
        path.parent.mkdir(mode=0o700, parents=True, exist_ok=True)
    else:
        ensure_private_temp_directory(path.parent)
    descriptor, temporary_name = tempfile.mkstemp(
        prefix=f".{path.name}.", dir=path.parent
    )
    temporary_path = pathlib.Path(temporary_name)
    try:
        os.fchmod(descriptor, mode)
        with os.fdopen(descriptor, "wb") as output:
            descriptor = -1
            output.write(content)
            output.flush()
            os.fsync(output.fileno())
        temporary_path.replace(path)
        return path
    except OSError as exception:
        raise InstallerError(f"Unable to write {path}: {exception}") from exception
    finally:
        if descriptor >= 0:
            os.close(descriptor)
        temporary_path.unlink(missing_ok=True)


def validate_https_response(response: Any, requested_url: str) -> None:
    """Reject a redirect that leaves HTTPS before trusting response bytes."""
    final_url = (
        response.geturl()
        if callable(getattr(response, "geturl", None))
        else requested_url
    )
    if not isinstance(final_url, str):
        final_url = requested_url
    if urllib.parse.urlsplit(final_url).scheme != "https":
        raise InstallerError(
            f"HTTPS request was redirected to an unsafe URL: {final_url}"
        )


def load_cached_install_config() -> InstallConfig | None:
    """Load validated interrupted-install input when it is available."""
    try:
        config_status = PENDING_CONFIG_PATH.lstat()
    except FileNotFoundError:
        return None
    except OSError as exception:
        raise InstallerError(
            f"Unable to inspect cached installer input {PENDING_CONFIG_PATH}: {exception}"
        ) from exception
    if not stat.S_ISREG(config_status.st_mode):
        raise InstallerError(
            f"Cached installer input must be a regular file: {PENDING_CONFIG_PATH}"
        )
    try:
        value = json.loads(
            read_limited_file(
                PENDING_CONFIG_PATH, MAX_MANIFEST_BYTES, "cached installer input"
            ).decode("utf-8")
        )
        return InstallConfig(
            base_url=normalize_base_url(str(value["baseUrl"])),
            admin_user=str(value["adminUser"]).strip(),
            admin_password=str(value["adminPassword"]),
            encryption_key=str(value["encryptionKey"]),
            encryption_iv=str(value["encryptionIv"]),
            token_secret=str(value["tokenSecret"]),
        )
    except (
        OSError,
        KeyError,
        TypeError,
        ValueError,
        json.JSONDecodeError,
        InstallerError,
        UnicodeDecodeError,
    ) as exception:
        print(f"Ignoring invalid cached installer input: {exception}")
        return None


def save_cached_install_config(config: InstallConfig) -> None:
    """Persist installer input atomically with owner-only permissions."""
    INSTALLER_STATE_ROOT.mkdir(mode=0o750, parents=True, exist_ok=True)
    INSTALLER_STATE_ROOT.chmod(0o750)
    atomic_write_file(
        PENDING_CONFIG_PATH,
        (json.dumps(config.to_json_value(), indent=2) + "\n").encode("utf-8"),
        0o600,
    )


def prompt_value(prompt: str, default: str) -> str:
    """Prompt for a non-empty text value and display its default."""
    while True:
        try:
            value = input(f"{prompt} [{default}]: ").strip()
        except EOFError as exception:
            raise InstallerError(
                "Interactive input is unavailable; use --quiet for automation."
            ) from exception
        resolved = value or default
        if resolved:
            return resolved
        print("A value is required.")


def collect_install_config(quiet: bool) -> InstallConfig:
    """Collect install settings, reusing cached values after an interruption."""
    cached = load_cached_install_config()
    generated_admin_password = secrets.token_urlsafe(24) if cached is None else None
    defaults = cached or InstallConfig(
        base_url=DEFAULT_BASE_URL,
        admin_user=DEFAULT_ADMIN_USER,
        admin_password=generated_admin_password or "",
        encryption_key=secrets.token_hex(32),
        encryption_iv=secrets.token_hex(16),
        token_secret=secrets.token_hex(32),
    )
    if quiet:
        config = defaults
    else:
        while True:
            try:
                base_url = normalize_base_url(
                    prompt_value("Base URL", defaults.base_url)
                )
                break
            except InstallerError as exception:
                print(exception)
        admin_user = prompt_value("Administrator username", defaults.admin_user)
        try:
            password = getpass.getpass(
                "Administrator password [press Enter to reuse the saved/default password]: "
            )
        except EOFError as exception:
            raise InstallerError(
                "Interactive password input is unavailable."
            ) from exception
        config = dataclasses.replace(
            defaults,
            base_url=base_url,
            admin_user=admin_user,
            admin_password=password or defaults.admin_password,
        )
    if not config.admin_user or not config.admin_password:
        raise InstallerError("Administrator username and password cannot be empty.")
    print("Installation settings:")
    print(f"  Base URL: {config.base_url}")
    print(f"  Administrator: {config.admin_user}")
    if (
        generated_admin_password is not None
        and config.admin_password == generated_admin_password
    ):
        print(f"  Generated administrator password: {generated_admin_password}")
        print("  Store this password securely; it will not be shown again.")
    else:
        print("  Administrator password: [hidden]")
    print("  Encryption and token secrets: [generated and hidden]")
    return config


def fetch_update_manifest() -> UpdateManifest:
    """Download and validate only the update metadata used by this installer."""
    request = urllib.request.Request(
        LATEST_MANIFEST_URL, headers={"User-Agent": "UzonMail-Linux-Installer/1"}
    )
    try:
        with urllib.request.urlopen(
            request, timeout=NETWORK_TIMEOUT_SECONDS
        ) as response:
            validate_https_response(response, LATEST_MANIFEST_URL)
            content = response.read(MAX_MANIFEST_BYTES + 1)
    except (OSError, urllib.error.URLError) as exception:
        raise InstallerError(
            f"Unable to download the update manifest: {exception}"
        ) from exception
    if len(content) > MAX_MANIFEST_BYTES:
        raise InstallerError("The update manifest exceeds the allowed size.")
    try:
        value = json.loads(content.decode("utf-8"))
    except (UnicodeDecodeError, json.JSONDecodeError) as exception:
        raise InstallerError(
            f"The update manifest is not valid JSON: {exception}"
        ) from exception
    if not isinstance(value, dict) or value.get("name") != APPLICATION_NAME:
        raise InstallerError("The update manifest does not describe UzonMail.")
    version = Version.parse(str(value.get("version", "")), "manifest version")
    environment_value = value.get("env")
    if not isinstance(environment_value, dict) or not environment_value:
        raise InstallerError(
            "The update manifest does not contain runtime requirements."
        )
    environment: dict[str, Version] = {}
    for framework_name, framework_version in environment_value.items():
        if framework_name not in SUPPORTED_FRAMEWORKS:
            raise InstallerError(
                f"Unsupported .NET framework requirement: {framework_name}"
            )
        environment[framework_name] = Version.parse(
            str(framework_version), f"runtime version for {framework_name}"
        )
    artifacts = value.get("artifacts")
    linux_artifact = (
        artifacts.get(LINUX_RUNTIME_IDENTIFIER) if isinstance(artifacts, dict) else None
    )
    if not isinstance(linux_artifact, dict):
        raise InstallerError(
            f"The update manifest does not contain artifacts.{LINUX_RUNTIME_IDENTIFIER}."
        )
    package_url = str(linux_artifact.get("url", ""))
    parsed_package_url = urllib.parse.urlsplit(package_url)
    if parsed_package_url.scheme != "https" or not parsed_package_url.netloc:
        raise InstallerError("The Linux update package URL must use HTTPS.")
    package_sha256 = str(linux_artifact.get("sha256", ""))
    if not SHA256_PATTERN.fullmatch(package_sha256):
        raise InstallerError("The Linux update package SHA-256 is invalid.")
    minimum_value = value.get("minCompatibleVersion")
    minimum_version = (
        Version.parse(str(minimum_value), "minimum compatible version")
        if minimum_value is not None
        else None
    )
    return UpdateManifest(
        version, environment, package_url, package_sha256, minimum_version
    )


def download_file(
    url: str,
    destination: pathlib.Path,
    quiet: bool,
    maximum_bytes: int = MAX_PACKAGE_BYTES,
    expected_sha256: str | None = None,
) -> None:
    """Download a file atomically while reporting bounded progress."""
    del quiet
    temp_root = require_temp_root()
    try:
        destination.relative_to(temp_root)
    except ValueError:
        destination.parent.mkdir(mode=0o700, parents=True, exist_ok=True)
    else:
        ensure_private_temp_directory(destination.parent)
    partial_path = destination.with_suffix(destination.suffix + ".part")
    request = urllib.request.Request(
        url, headers={"User-Agent": "UzonMail-Linux-Installer/1"}
    )
    try:
        with urllib.request.urlopen(
            request, timeout=NETWORK_TIMEOUT_SECONDS
        ) as response:
            validate_https_response(response, url)
            total_text = response.headers.get("Content-Length")
            total = int(total_text) if total_text and total_text.isdigit() else None
            if total is not None and total > maximum_bytes:
                raise InstallerError(
                    f"Download exceeds the {maximum_bytes}-byte limit: {url}"
                )
            downloaded = 0
            next_report = 10
            digest = hashlib.sha256()
            with partial_path.open("wb") as output:
                while True:
                    chunk = response.read(1024 * 1024)
                    if not chunk:
                        break
                    output.write(chunk)
                    digest.update(chunk)
                    downloaded += len(chunk)
                    if downloaded > maximum_bytes:
                        raise InstallerError(
                            f"Download exceeds the {maximum_bytes}-byte limit: {url}"
                        )
                    if total:
                        percentage = downloaded * 100 // total
                        if percentage >= next_report:
                            print(f"   Downloaded {min(percentage, 100)}%")
                            next_report += 10
        actual_sha256 = digest.hexdigest()
        if expected_sha256 is not None and actual_sha256 != expected_sha256:
            raise InstallerError(
                f"Downloaded file SHA-256 mismatch for {url}: expected "
                f"{expected_sha256}, got {actual_sha256}."
            )
        partial_path.replace(destination)
    except InstallerError:
        partial_path.unlink(missing_ok=True)
        raise
    except (OSError, urllib.error.URLError) as exception:
        partial_path.unlink(missing_ok=True)
        raise InstallerError(f"Unable to download {url}: {exception}") from exception


def safe_extract_package(
    archive_path: pathlib.Path, destination: pathlib.Path, quiet: bool
) -> pathlib.Path:
    """Extract a release archive after rejecting unsafe ZIP entries."""
    del quiet
    if destination.exists():
        if destination.is_symlink() or not destination.is_dir():
            raise InstallerError(f"Extraction destination is unsafe: {destination}")
        shutil.rmtree(destination)
    destination.mkdir(mode=0o700, parents=True)
    try:
        with zipfile.ZipFile(archive_path) as archive:
            package_entries: list[zipfile.ZipInfo] = []
            extracted_bytes = 0
            seen_entries: set[str] = set()
            if len(archive.infolist()) > MAX_ARCHIVE_ENTRIES:
                raise InstallerError(
                    f"The archive contains more than {MAX_ARCHIVE_ENTRIES} entries."
                )
            for entry in archive.infolist():
                normalized = entry.filename.replace("\\", "/")
                path_parts = pathlib.PurePosixPath(normalized).parts
                file_mode = entry.external_attr >> 16
                file_type = stat.S_IFMT(file_mode)
                if (
                    normalized.startswith("/")
                    or ".." in path_parts
                    or (file_type not in {0, stat.S_IFREG, stat.S_IFDIR})
                    or normalized in seen_entries
                ):
                    raise InstallerError(f"Unsafe archive entry: {entry.filename}")
                seen_entries.add(normalized)
                if path_parts and path_parts[0] == PACKAGE_DIRECTORY_NAME:
                    package_entries.append(entry)
                    extracted_bytes += entry.file_size
                    if extracted_bytes > MAX_EXTRACTED_BYTES:
                        raise InstallerError(
                            f"Extracted package exceeds the {MAX_EXTRACTED_BYTES}-byte limit."
                        )
            if not package_entries:
                raise InstallerError(
                    f"The archive does not contain {PACKAGE_DIRECTORY_NAME}/."
                )
            archive.extractall(destination, members=package_entries)
    except (OSError, zipfile.BadZipFile) as exception:
        raise InstallerError(
            f"Unable to extract the release archive: {exception}"
        ) from exception
    package_root = destination / PACKAGE_DIRECTORY_NAME
    if not (package_root / SERVICE_ASSEMBLY_NAME).is_file():
        raise InstallerError(
            f"The archive does not contain {PACKAGE_DIRECTORY_NAME}/{SERVICE_ASSEMBLY_NAME}."
        )
    return package_root


def _read_u16(content: bytes, offset: int) -> int:
    return struct.unpack_from("<H", content, offset)[0]


def _read_u32(content: bytes, offset: int) -> int:
    return struct.unpack_from("<I", content, offset)[0]


def _read_u64(content: bytes, offset: int) -> int:
    return struct.unpack_from("<Q", content, offset)[0]


def read_dotnet_assembly_version(assembly_path: pathlib.Path) -> Version:
    """Read AssemblyVersion directly from a managed PE file using ECMA-335 metadata."""
    try:
        content = read_limited_file(
            assembly_path, MAX_ASSEMBLY_BYTES, ".NET service assembly"
        )
        if content[:2] != b"MZ":
            raise InstallerError(f"Not a PE assembly: {assembly_path}")
        pe_offset = _read_u32(content, 0x3C)
        if content[pe_offset : pe_offset + 4] != b"PE\0\0":
            raise InstallerError(f"Invalid PE signature: {assembly_path}")
        coff_offset = pe_offset + 4
        section_count = _read_u16(content, coff_offset + 2)
        optional_size = _read_u16(content, coff_offset + 16)
        optional_offset = coff_offset + 20
        magic = _read_u16(content, optional_offset)
        data_directory_offset = optional_offset + (96 if magic == 0x10B else 112)
        if magic not in {0x10B, 0x20B}:
            raise InstallerError(f"Unsupported PE format: {assembly_path}")
        cli_rva = _read_u32(content, data_directory_offset + 14 * 8)
        section_offset = optional_offset + optional_size

        def rva_to_offset(rva: int) -> int:
            for index in range(section_count):
                current = section_offset + index * 40
                virtual_size = _read_u32(content, current + 8)
                virtual_address = _read_u32(content, current + 12)
                raw_size = _read_u32(content, current + 16)
                raw_pointer = _read_u32(content, current + 20)
                if (
                    virtual_address
                    <= rva
                    < virtual_address + max(virtual_size, raw_size)
                ):
                    return raw_pointer + rva - virtual_address
            raise InstallerError(f"PE RVA is outside all sections: {assembly_path}")

        cli_offset = rva_to_offset(cli_rva)
        metadata_rva = _read_u32(content, cli_offset + 8)
        metadata_offset = rva_to_offset(metadata_rva)
        if content[metadata_offset : metadata_offset + 4] != b"BSJB":
            raise InstallerError(f"Invalid CLI metadata signature: {assembly_path}")
        version_length = _read_u32(content, metadata_offset + 12)
        stream_count_offset = metadata_offset + 16 + ((version_length + 3) & ~3)
        stream_count = _read_u16(content, stream_count_offset + 2)
        header_offset = stream_count_offset + 4
        streams: dict[str, tuple[int, int]] = {}
        for _ in range(stream_count):
            stream_relative_offset = _read_u32(content, header_offset)
            stream_size = _read_u32(content, header_offset + 4)
            name_offset = header_offset + 8
            name_end = content.index(b"\0", name_offset)
            stream_name = content[name_offset:name_end].decode("ascii")
            streams[stream_name] = (
                metadata_offset + stream_relative_offset,
                stream_size,
            )
            header_offset = name_offset + ((name_end - name_offset + 1 + 3) & ~3)
        tables_offset = streams.get("#~", streams.get("#-", (0, 0)))[0]
        if tables_offset == 0:
            raise InstallerError(
                f"Assembly metadata tables are missing: {assembly_path}"
            )
        heap_sizes = content[tables_offset + 6]
        valid_mask = _read_u64(content, tables_offset + 8)
        row_counts: dict[int, int] = {}
        rows_offset = tables_offset + 24
        for table_index in range(64):
            if valid_mask & (1 << table_index):
                row_counts[table_index] = _read_u32(content, rows_offset)
                rows_offset += 4
        if row_counts.get(32, 0) != 1:
            raise InstallerError(f"Assembly metadata row is missing: {assembly_path}")
        assembly_offset = rows_offset
        for table_index in range(32):
            assembly_offset += row_counts.get(table_index, 0) * _metadata_row_size(
                table_index, row_counts, heap_sizes
            )
        return Version(
            _read_u16(content, assembly_offset + 4),
            _read_u16(content, assembly_offset + 6),
            _read_u16(content, assembly_offset + 8),
            _read_u16(content, assembly_offset + 10),
        )
    except (
        IndexError,
        UnicodeDecodeError,
        struct.error,
        ValueError,
    ) as exception:
        raise InstallerError(
            f"Unable to read assembly version from {assembly_path}: {exception}"
        ) from exception


def _metadata_row_size(table: int, rows: dict[int, int], heap_sizes: int) -> int:
    """Return the ECMA-335 row size for metadata tables preceding Assembly."""
    string_size = 4 if heap_sizes & 0x01 else 2
    guid_size = 4 if heap_sizes & 0x02 else 2
    blob_size = 4 if heap_sizes & 0x04 else 2

    def table_index(target: int) -> int:
        return 4 if rows.get(target, 0) >= 65536 else 2

    def coded(tag_bits: int, targets: Sequence[int]) -> int:
        maximum = max((rows.get(target, 0) for target in targets), default=0)
        return 4 if maximum >= (1 << (16 - tag_bits)) else 2

    type_def_or_ref = coded(2, (2, 1, 27))
    schemas: dict[int, tuple[int, ...]] = {
        0: (2, string_size, guid_size, guid_size, guid_size),
        1: (coded(2, (0, 26, 35, 1)), string_size, string_size),
        2: (
            4,
            string_size,
            string_size,
            type_def_or_ref,
            table_index(4),
            table_index(6),
        ),
        3: (table_index(4),),
        4: (2, string_size, blob_size),
        5: (table_index(6),),
        6: (4, 2, 2, string_size, blob_size, table_index(8)),
        7: (table_index(8),),
        8: (2, 2, string_size),
        9: (table_index(2), type_def_or_ref),
        10: (coded(3, (2, 1, 26, 6, 27)), string_size, blob_size),
        11: (2, coded(2, (4, 8, 23)), blob_size),
        12: (
            coded(
                5,
                (
                    6,
                    4,
                    1,
                    2,
                    8,
                    9,
                    10,
                    0,
                    14,
                    23,
                    20,
                    17,
                    26,
                    27,
                    32,
                    35,
                    38,
                    39,
                    40,
                    42,
                    44,
                    43,
                ),
            ),
            coded(3, (6, 10)),
            blob_size,
        ),
        13: (coded(1, (4, 8)), blob_size),
        14: (2, coded(2, (2, 6, 32)), blob_size),
        15: (2, 4, table_index(2)),
        16: (4, table_index(4)),
        17: (blob_size,),
        18: (table_index(2), table_index(20)),
        19: (table_index(20),),
        20: (2, string_size, type_def_or_ref),
        21: (table_index(2), table_index(23)),
        22: (table_index(23),),
        23: (2, string_size, blob_size),
        24: (2, table_index(6), coded(1, (20, 23))),
        25: (table_index(2), coded(1, (6, 10)), coded(1, (6, 10))),
        26: (string_size,),
        27: (blob_size,),
        28: (2, coded(1, (4, 6)), string_size, table_index(26)),
        29: (4, table_index(4)),
        30: (4, 4),
        31: (4,),
    }
    if table not in schemas:
        raise InstallerError(f"Unsupported metadata table before Assembly: {table}")
    return sum(schemas[table])


def get_installed_version(root: pathlib.Path = INSTALL_ROOT) -> Version:
    """Read the installed UzonMail version from its managed service assembly."""
    assembly_path = root / SERVICE_ASSEMBLY_NAME
    if not assembly_path.is_file():
        raise InstallerError(f"UzonMail is not installed at {root}.")
    return read_dotnet_assembly_version(assembly_path)


def read_installed_runtimes(
    dotnet_path: pathlib.Path | str | None = None,
) -> dict[str, list[Version]]:
    """Read installed shared frameworks from dotnet without changing the system."""
    dotnet = (
        os.fspath(dotnet_path) if dotnet_path is not None else shutil.which("dotnet")
    )
    if dotnet is None:
        return {}
    try:
        result = subprocess.run(
            [dotnet, "--list-runtimes"], check=True, text=True, capture_output=True
        )
    except (OSError, subprocess.CalledProcessError):
        return {}
    runtimes: dict[str, list[Version]] = {}
    pattern = re.compile(r"^(\S+)\s+(\d+(?:\.\d+){1,3})\s+\[")
    for line in result.stdout.splitlines():
        match = pattern.match(line.strip())
        if match:
            try:
                runtimes.setdefault(match.group(1), []).append(
                    Version.parse(match.group(2), "installed runtime version")
                )
            except InstallerError:
                continue
    return runtimes


def runtime_requirement_is_met(installed: Iterable[Version], required: Version) -> bool:
    """Apply .NET's compatible same-major patch/minor runtime rule."""
    return any(
        version.major == required.major and version >= required for version in installed
    )


def runtime_requirements_are_met(
    installed: dict[str, list[Version]], requirements: dict[str, Version]
) -> bool:
    """Return whether one dotnet host exposes every required shared framework."""
    return all(
        runtime_requirement_is_met(installed.get(name, ()), version)
        for name, version in requirements.items()
    )


def normalize_dotnet_path(value: pathlib.Path | str) -> pathlib.Path:
    """Validate a root-controlled dotnet host before storing it in a systemd unit."""
    path = pathlib.Path(value).expanduser()
    try:
        resolved = path.resolve(strict=True)
        file_status = resolved.stat()
    except OSError as exception:
        raise InstallerError(f"Invalid dotnet host {path}: {exception}") from exception
    if (
        not resolved.is_absolute()
        or not stat.S_ISREG(file_status.st_mode)
        or any(character.isspace() for character in os.fspath(resolved))
    ):
        raise InstallerError(f"dotnet host is not a regular absolute file: {resolved}")
    if not os.access(resolved, os.X_OK):
        raise InstallerError(f"dotnet host is not executable: {resolved}")
    for trusted_path in (resolved, *resolved.parents):
        try:
            trusted_status = trusted_path.stat()
        except OSError as exception:
            raise InstallerError(
                f"Unable to validate dotnet host path {trusted_path}: {exception}"
            ) from exception
        if trusted_status.st_uid != 0 or trusted_status.st_mode & 0o022:
            raise InstallerError(
                f"dotnet host path must be root-owned and not writable by other users: "
                f"{trusted_path}"
            )
    return resolved


def parse_stored_dotnet_path(value: Any) -> pathlib.Path:
    """Parse an absolute state path without requiring the old host to still exist."""
    if not isinstance(value, str):
        raise InstallerError("Installer state has an invalid dotnetPath value.")
    posix_path = pathlib.PurePosixPath(value)
    path = pathlib.Path(value)
    if (
        not posix_path.is_absolute()
        or any(part == ".." for part in posix_path.parts)
        or "\\" in value
        or any(character.isspace() for character in value)
    ):
        raise InstallerError(
            f"Installer state has an invalid dotnetPath value: {value!r}"
        )
    return path


def verify_dotnet_install_script(
    installer_path: pathlib.Path,
    signature_path: pathlib.Path,
    key_path: pathlib.Path,
    runner: CommandRunner,
) -> None:
    """Verify Microsoft's signing key and detached script signature before execution."""
    if shutil.which("gpg") is None:
        raise InstallerError(
            "gpg is required to verify dotnet-install.sh. Install gnupg and retry."
        )
    gpg_home = require_temp_root() / "gnupg"
    gpg_home.mkdir(mode=0o700)
    runner.run(
        ["gpg", "--homedir", gpg_home, "--batch", "--import", key_path],
        description="Import the Microsoft dotnet-install signing key",
    )
    fingerprint_result = runner.run(
        [
            "gpg",
            "--homedir",
            gpg_home,
            "--batch",
            "--with-colons",
            "--fingerprint",
        ],
        description="Read the imported Microsoft signing key fingerprint",
        capture_output=True,
    )
    fingerprints = {
        fields[9]
        for line in fingerprint_result.stdout.splitlines()
        if (fields := line.split(":"))[0] == "fpr" and len(fields) > 9
    }
    if DOTNET_INSTALL_KEY_FINGERPRINT not in fingerprints:
        raise InstallerError(
            "The downloaded dotnet-install signing key has an unexpected fingerprint."
        )
    signature_result = runner.run(
        [
            "gpg",
            "--homedir",
            gpg_home,
            "--batch",
            "--status-fd=1",
            "--verify",
            signature_path,
            installer_path,
        ],
        description="Verify the dotnet-install.sh signature",
        capture_output=True,
    )
    has_expected_valid_signature = any(
        DOTNET_INSTALL_KEY_FINGERPRINT in fields[2:]
        for line in signature_result.stdout.splitlines()
        if line.startswith("[GNUPG:] VALIDSIG ") and len(fields := line.split()) > 2
    )
    if not has_expected_valid_signature:
        raise InstallerError(
            "dotnet-install.sh does not have the expected valid signature."
        )


def ensure_dotnet_environment(
    manifest: UpdateManifest, runner: CommandRunner
) -> pathlib.Path:
    """Return a host satisfying all requirements, installing an isolated host if needed."""
    system_dotnet = shutil.which("dotnet")
    if system_dotnet is not None:
        system_path = normalize_dotnet_path(system_dotnet)
        if runtime_requirements_are_met(
            read_installed_runtimes(system_path), manifest.environment
        ):
            print(f"Required .NET runtimes are available through {system_path}.")
            return system_path

    managed_installed = (
        read_installed_runtimes(MANAGED_DOTNET_PATH)
        if MANAGED_DOTNET_PATH.is_file()
        else {}
    )
    if runtime_requirements_are_met(managed_installed, manifest.environment):
        managed_path = normalize_dotnet_path(MANAGED_DOTNET_PATH)
        print(f"Required .NET runtimes are available through {managed_path}.")
        return managed_path

    temp_root = require_temp_root()
    installer_path = temp_root / "dotnet-install.sh"
    signature_path = temp_root / "dotnet-install.sig"
    key_path = temp_root / "dotnet-install.asc"
    download_file(
        DOTNET_INSTALL_URL,
        installer_path,
        runner.quiet,
        maximum_bytes=MAX_MANIFEST_BYTES,
    )
    download_file(
        DOTNET_INSTALL_SIGNATURE_URL,
        signature_path,
        runner.quiet,
        maximum_bytes=MAX_MANIFEST_BYTES,
    )
    download_file(
        DOTNET_INSTALL_KEY_URL,
        key_path,
        runner.quiet,
        maximum_bytes=MAX_MANIFEST_BYTES,
    )
    verify_dotnet_install_script(installer_path, signature_path, key_path, runner)
    installer_path.chmod(0o700)
    ordered_names = sorted(
        manifest.environment, key=lambda name: name != "Microsoft.AspNetCore.App"
    )
    for framework_name in ordered_names:
        installed = (
            read_installed_runtimes(MANAGED_DOTNET_PATH)
            if MANAGED_DOTNET_PATH.is_file()
            else {}
        )
        required = manifest.environment[framework_name]
        if runtime_requirement_is_met(installed.get(framework_name, ()), required):
            continue
        runner.run(
            [
                "bash",
                installer_path,
                "--runtime",
                SUPPORTED_FRAMEWORKS[framework_name],
                "--version",
                required.to_runtime_string(),
                "--install-dir",
                DOTNET_INSTALL_ROOT,
                "--no-path",
            ],
            description=f"Install {framework_name} {required}",
        )
    managed_path = normalize_dotnet_path(MANAGED_DOTNET_PATH)
    installed = read_installed_runtimes(managed_path)
    unresolved = [
        f"{name} {version}"
        for name, version in manifest.environment.items()
        if not runtime_requirement_is_met(installed.get(name, ()), version)
    ]
    if unresolved:
        raise InstallerError(
            f"Required runtimes are still missing: {', '.join(unresolved)}"
        )
    return managed_path


def strip_json_comments(content: str) -> str:
    """Remove JSON line and block comments while preserving quoted strings."""
    output: list[str] = []
    index = 0
    in_string = False
    escaped = False
    while index < len(content):
        current = content[index]
        following = content[index + 1] if index + 1 < len(content) else ""
        if in_string:
            output.append(current)
            if escaped:
                escaped = False
            elif current == "\\":
                escaped = True
            elif current == '"':
                in_string = False
            index += 1
            continue
        if current == '"':
            in_string = True
            output.append(current)
            index += 1
            continue
        if current == "/" and following == "/":
            index += 2
            while index < len(content) and content[index] not in "\r\n":
                index += 1
            continue
        if current == "/" and following == "*":
            end = content.find("*/", index + 2)
            if end < 0:
                raise InstallerError(
                    "The package appsettings.json contains an unterminated comment."
                )
            index = end + 2
            continue
        output.append(current)
        index += 1
    return "".join(output)


def create_production_config(
    package_root: pathlib.Path, config: InstallConfig
) -> dict[str, Any]:
    """Create minimal production overrides while retaining package CORS defaults."""
    default_path = package_root / "appsettings.json"
    try:
        defaults = json.loads(
            strip_json_comments(default_path.read_text(encoding="utf-8"))
        )
    except (OSError, json.JSONDecodeError) as exception:
        raise InstallerError(
            f"Unable to read package defaults: {exception}"
        ) from exception
    default_cors = defaults.get("Cors", [])
    if not isinstance(default_cors, list) or any(
        not isinstance(value, str) for value in default_cors
    ):
        raise InstallerError("The package contains an invalid Cors setting.")
    cors = list(dict.fromkeys([*default_cors, get_url_origin(config.base_url)]))
    return {
        "BaseUrl": config.base_url,
        "Cors": cors,
        "User": {
            "AdminUser": {
                "UserId": config.admin_user,
                "Password": config.admin_password,
            }
        },
        "EncryptParams": {"Key": config.encryption_key, "IV": config.encryption_iv},
        "TokenParams": {
            "Secret": config.token_secret,
            "Issuer": config.base_url,
            "Audience": APPLICATION_NAME,
            "Expire": DEFAULT_TOKEN_EXPIRATION_MILLISECONDS,
        },
    }


def write_temporary_json(filename: str, value: Any, mode: int = 0o600) -> pathlib.Path:
    """Write a JSON document under the protected installer temporary directory."""
    return atomic_write_file(
        require_temp_root() / filename,
        (json.dumps(value, indent=2) + "\n").encode("utf-8"),
        mode,
    )


def service_unit_content(dotnet_path: pathlib.Path) -> str:
    """Return the systemd unit for the dedicated UzonMail service account."""
    install_root = INSTALL_ROOT.as_posix()
    service_home = SERVICE_HOME.as_posix()
    dotnet_command = normalize_dotnet_path(dotnet_path).as_posix()
    return f"""[Unit]
Description=UzonMail Service
Wants=network-online.target
After=network-online.target

[Service]
Type=simple
User={SERVICE_USER}
Group={SERVICE_USER}
WorkingDirectory={install_root}
ExecStart={dotnet_command} {install_root}/{SERVICE_ASSEMBLY_NAME}
Restart=always
RestartSec=10
SyslogIdentifier=uzon-mail
UMask=0027
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=HOME={service_home}
NoNewPrivileges=true
PrivateTmp=true
ProtectSystem=strict
ProtectHome=true
ProtectKernelTunables=true
ProtectKernelModules=true
ProtectControlGroups=true
RestrictSUIDSGID=true
LockPersonality=true
CapabilityBoundingSet=
ReadWritePaths={install_root}/data {install_root}/logs -{install_root}/wwwroot/app.config.json

[Install]
WantedBy=multi-user.target
"""


def existing_service_user_is_valid() -> bool:
    """Validate the dedicated account when it already exists."""
    import grp
    import pwd

    try:
        account = pwd.getpwnam(SERVICE_USER)
    except KeyError:
        account = None
    if account is None:
        try:
            grp.getgrnam(SERVICE_USER)
        except KeyError:
            return False
        raise InstallerError(
            f"Group '{SERVICE_USER}' exists without its service account. Inspect or remove "
            "the group before installation."
        )
    try:
        primary_group = grp.getgrgid(account.pw_gid)
    except KeyError as exception:
        raise InstallerError(
            f"Primary group for existing account '{SERVICE_USER}' does not exist."
        ) from exception
    shell_name = pathlib.Path(account.pw_shell).name
    supplementary_groups = [
        group.gr_name
        for group in grp.getgrall()
        if SERVICE_USER in group.gr_mem and group.gr_gid != account.pw_gid
    ]
    if (
        account.pw_uid == 0
        or account.pw_gid == 0
        or primary_group.gr_name != SERVICE_USER
        or shell_name not in {"nologin", "false"}
        or supplementary_groups
    ):
        raise InstallerError(
            f"Existing account '{SERVICE_USER}' must be unprivileged, use only primary "
            f"group '{SERVICE_USER}', a nologin/false shell, and no supplementary groups."
        )
    return True


def ensure_service_user(runner: CommandRunner) -> None:
    """Create the dedicated account when validation confirms that it is absent."""
    if existing_service_user_is_valid():
        return
    nologin = shutil.which("nologin") or shutil.which("false") or "/bin/false"
    runner.run(
        [
            "useradd",
            "--system",
            "--user-group",
            "--home-dir",
            SERVICE_HOME,
            "--no-create-home",
            "--shell",
            nologin,
            SERVICE_USER,
        ],
        description=f"Create system account '{SERVICE_USER}' with runtime home {SERVICE_HOME}",
    )


def validate_service_user() -> None:
    """Require the dedicated account before mutating an installed application."""
    if not existing_service_user_is_valid():
        raise InstallerError(f"System account '{SERVICE_USER}' does not exist.")


def install_json_file(
    runner: CommandRunner,
    source: pathlib.Path,
    destination: pathlib.Path,
    description: str,
    *,
    mode: str = "0640",
    group: str = SERVICE_USER,
) -> None:
    """Install a generated JSON file with explicit ownership and permissions."""
    runner.run(
        ["install", "-m", mode, "-o", "root", "-g", group, source, destination],
        description=description,
    )


def create_install_state(
    manifest: UpdateManifest,
    dotnet_path: pathlib.Path,
) -> InstallState:
    """Create the authoritative state for a completed release."""
    return InstallState(
        version=manifest.version,
        minimum_compatible_version=manifest.minimum_compatible_version,
        dotnet_path=normalize_dotnet_path(dotnet_path),
        installed_at=dt.datetime.now(dt.timezone.utc),
    )


def write_install_state(state: InstallState, runner: CommandRunner) -> None:
    """Persist the installed version and restore compatibility boundary."""
    runner.run(
        [
            "install",
            "-d",
            "-m",
            "0750",
            "-o",
            "root",
            "-g",
            "root",
            INSTALLER_STATE_ROOT,
        ],
        description=f"Create installer state directory {INSTALLER_STATE_ROOT}",
    )
    temporary = write_temporary_json("install-state.json", state.to_json_value(), 0o600)
    install_json_file(
        runner,
        temporary,
        INSTALL_STATE_PATH,
        f"Record installation state at {INSTALL_STATE_PATH}",
        mode="0640",
        group="root",
    )


def read_install_state() -> InstallState | None:
    """Read the single supported installer state schema."""
    if not INSTALL_STATE_PATH.is_file():
        return None
    try:
        value = json.loads(
            read_limited_file(
                INSTALL_STATE_PATH, MAX_MANIFEST_BYTES, "installer state"
            ).decode("utf-8")
        )
        if value.get("schemaVersion") != INSTALL_STATE_SCHEMA_VERSION:
            raise InstallerError(
                f"Unsupported installer state schema in {INSTALL_STATE_PATH}."
            )
        minimum = value.get("minCompatibleVersion")
        installed_at = dt.datetime.fromisoformat(str(value["installedAt"]))
        if installed_at.tzinfo is None:
            raise InstallerError("Installer state timestamp must include a timezone.")
        return InstallState(
            version=Version.parse(str(value["version"]), "installed state version"),
            minimum_compatible_version=(
                Version.parse(str(minimum), "installed minimum compatible version")
                if minimum is not None
                else None
            ),
            dotnet_path=parse_stored_dotnet_path(value["dotnetPath"]),
            installed_at=installed_at,
        )
    except InstallerError:
        raise
    except (
        KeyError,
        TypeError,
        ValueError,
        UnicodeDecodeError,
        json.JSONDecodeError,
    ) as exception:
        raise InstallerError(
            f"Invalid installer state {INSTALL_STATE_PATH}: {exception}"
        ) from exception


def require_install_state(installed_version: Version) -> InstallState:
    """Require state matching the release found in the installation directory."""
    state = read_install_state()
    if state is None:
        raise InstallerError(
            f"Installer state is missing at {INSTALL_STATE_PATH}; inspect this installation manually."
        )
    if state.version != installed_version:
        raise InstallerError(
            f"Installer state version {state.version} does not match installed version "
            f"{installed_version}. Inspect the installation manually."
        )
    return state


def prepare_package(manifest: UpdateManifest, runner: CommandRunner) -> pathlib.Path:
    """Download, safely extract, and version-check the latest Linux package."""
    temp_root = require_temp_root()
    archive = temp_root / f"uzonmail-service-linux-x64-{manifest.version}.zip"
    extraction_root = temp_root / "extracted" / str(manifest.version)
    download_file(
        manifest.package_url,
        archive,
        runner.quiet,
        expected_sha256=manifest.package_sha256,
    )
    package_root = safe_extract_package(archive, extraction_root, runner.quiet)
    package_version = read_dotnet_assembly_version(package_root / SERVICE_ASSEMBLY_NAME)
    if package_version != manifest.version:
        raise InstallerError(
            f"Package version {package_version} does not match manifest version {manifest.version}."
        )
    return package_root


def set_installation_permissions(root: pathlib.Path, runner: CommandRunner) -> None:
    """Grant the service account access only to runtime-writable application paths."""
    validate_mutable_installation_paths(root, runner)
    runner.run(
        ["chown", "-R", "--no-dereference", "root:root", root],
        description=f"Assign release files under {root} to root",
    )
    runner.run(
        ["chmod", "-R", "u=rwX,go=rX", root],
        description=f"Remove non-root write access under {root}",
    )
    for relative_name in WRITABLE_DIRECTORY_NAMES:
        path = root / relative_name
        runner.run(
            [
                "install",
                "-d",
                "-m",
                "0750",
                "-o",
                SERVICE_USER,
                "-g",
                SERVICE_USER,
                path,
            ],
            description=f"Prepare writable directory {path}",
        )
        runner.run(
            [
                "find",
                path,
                "-xdev",
                "-exec",
                "chown",
                "--no-dereference",
                f"{SERVICE_USER}:{SERVICE_USER}",
                "{}",
                "+",
            ],
            description=f"Assign {path} to the service account",
        )
        runner.run(
            ["find", path, "-xdev", "-exec", "chmod", "u=rwX,g=rX,o=", "{}", "+"],
            description=f"Restrict runtime data permissions under {path}",
        )
    frontend_config = root / "wwwroot/app.config.json"
    if frontend_config.exists():
        runner.run(
            ["chown", f"{SERVICE_USER}:{SERVICE_USER}", frontend_config],
            description=f"Allow the service to update {frontend_config}",
        )
        runner.run(
            ["chmod", "0640", frontend_config],
            description=f"Restrict permissions on {frontend_config}",
        )
    production_config = root / PRODUCTION_CONFIG_NAME
    if production_config.exists():
        runner.run(
            ["chown", f"root:{SERVICE_USER}", production_config],
            description=f"Assign secure ownership to {production_config}",
        )
        runner.run(
            ["chmod", "0640", production_config],
            description=f"Restrict permissions on {production_config}",
        )


def install_service_unit(dotnet_path: pathlib.Path, runner: CommandRunner) -> None:
    """Install and reload the unit for the selected dotnet host."""
    unit_source = atomic_write_file(
        require_temp_root() / SERVICE_NAME,
        service_unit_content(dotnet_path).encode("utf-8"),
        0o644,
    )
    install_json_file(
        runner,
        unit_source,
        SERVICE_UNIT_PATH,
        f"Register systemd unit {SERVICE_UNIT_PATH}",
        mode="0644",
        group="root",
    )
    runner.run(
        ["systemctl", "daemon-reload"],
        description="Reload systemd units",
    )


def register_and_start_service(
    dotnet_path: pathlib.Path, runner: CommandRunner
) -> None:
    """Install, enable, start, and verify the UzonMail systemd unit."""
    install_service_unit(dotnet_path, runner)
    runner.run(
        ["systemctl", "enable", SERVICE_NAME],
        description=f"Enable {SERVICE_NAME} at boot",
    )
    start_service(runner)


def is_service_active() -> bool:
    """Return whether the UzonMail systemd unit is currently active."""
    result = subprocess.run(
        ["systemctl", "is-active", "--quiet", SERVICE_NAME], capture_output=True
    )
    return result.returncode == 0


def is_service_enabled() -> bool:
    """Return whether the UzonMail unit is enabled for system startup."""
    result = subprocess.run(
        ["systemctl", "is-enabled", "--quiet", SERVICE_NAME], capture_output=True
    )
    return result.returncode == 0


def snapshot_system_file(source: pathlib.Path, snapshot: pathlib.Path) -> bool:
    """Copy a small root-readable system file into the private temporary directory."""
    if not source.is_file():
        return False
    content = read_limited_file(source, MAX_MANIFEST_BYTES, "system file")
    atomic_write_file(snapshot, content, 0o600)
    return True


def restore_system_file(
    snapshot: pathlib.Path,
    destination: pathlib.Path,
    runner: CommandRunner,
    *,
    existed: bool,
) -> None:
    """Restore or remove a snapshotted system file without another confirmation."""
    if existed:
        install_json_file(
            runner,
            snapshot,
            destination,
            f"Restore {destination}",
            mode="0644",
            group="root",
        )
    else:
        runner.run(
            ["rm", "-f", destination],
            description=f"Remove newly created {destination}",
        )


def restore_service_state(
    runner: CommandRunner, *, was_enabled: bool, was_active: bool
) -> None:
    """Restore systemd enablement and activity after a failed transaction."""
    runner.run(
        ["systemctl", "daemon-reload"],
        description="Reload restored systemd units",
    )
    runner.run(
        ["systemctl", "enable" if was_enabled else "disable", SERVICE_NAME],
        description=f"Restore enablement of {SERVICE_NAME}",
        check=False,
    )
    if was_active:
        start_service(runner)
    else:
        stop_service(runner)


def start_service(runner: CommandRunner) -> None:
    """Start UzonMail and verify that it remains active after initialization."""
    runner.run(
        ["systemctl", "start", SERVICE_NAME],
        description=f"Start {SERVICE_NAME}",
    )
    time.sleep(2)
    if not is_service_active():
        raise InstallerError(
            f"{SERVICE_NAME} did not remain active. Check 'journalctl -u {SERVICE_NAME}'."
        )


def stop_service(runner: CommandRunner) -> None:
    """Stop UzonMail when it is active."""
    if is_service_active():
        runner.run(
            ["systemctl", "stop", SERVICE_NAME],
            description=f"Stop {SERVICE_NAME}",
        )


def validate_backup_tree(
    path: pathlib.Path, runner: CommandRunner | None = None
) -> None:
    """Reject inaccessible trees, mount crossings, links, and special objects."""
    if runner is not None:
        result = runner.run(
            ["find", path, "-xdev", "-printf", "%D:%y\\0"],
            description=f"Validate persistent tree {path}",
            capture_output=True,
        )
        records = result.stdout.split("\0")
        if records and records[-1] == "":
            records.pop()
        if not records:
            raise InstallerError(f"Persistent directory is missing or empty: {path}")
        if len(records) > MAX_ARCHIVE_ENTRIES:
            raise InstallerError(
                f"Persistent tree contains more than {MAX_ARCHIVE_ENTRIES} entries: {path}"
            )
        try:
            root_device, root_type = records[0].split(":", maxsplit=1)
            parsed_records = [record.split(":", maxsplit=1) for record in records]
        except ValueError as exception:
            raise InstallerError(
                f"Unable to parse filesystem validation output for {path}."
            ) from exception
        if root_type != "d" or any(
            device != root_device or file_type not in {"d", "f"}
            for device, file_type in parsed_records
        ):
            raise InstallerError(
                f"Persistent tree contains a mount, link, or special file: {path}"
            )
        return

    try:
        root_status = path.lstat()
    except OSError as exception:
        raise InstallerError(
            f"Unable to inspect backup directory {path}: {exception}"
        ) from exception
    if not stat.S_ISDIR(root_status.st_mode):
        raise InstallerError(f"Backup directory is invalid: {path}")
    visited_entries = 0

    def on_walk_error(exception: OSError) -> None:
        raise InstallerError(
            f"Unable to traverse backup directory {path}: {exception}"
        ) from exception

    for root, directories, files in os.walk(
        path, followlinks=False, onerror=on_walk_error
    ):
        for name in [*directories, *files]:
            visited_entries += 1
            if visited_entries > MAX_ARCHIVE_ENTRIES:
                raise InstallerError(
                    f"Backup contains more than {MAX_ARCHIVE_ENTRIES} entries: {path}"
                )
            candidate = pathlib.Path(root) / name
            try:
                candidate_status = candidate.lstat()
            except OSError as exception:
                raise InstallerError(
                    f"Unable to inspect backup content {candidate}: {exception}"
                ) from exception
            if candidate_status.st_dev != root_status.st_dev or not (
                stat.S_ISDIR(candidate_status.st_mode)
                or stat.S_ISREG(candidate_status.st_mode)
            ):
                raise InstallerError(
                    f"Backup contains a mount, link, or special file: {candidate}"
                )


def validate_persistent_source(
    root: pathlib.Path, runner: CommandRunner | None = None
) -> None:
    """Reject runtime content that root must not archive or copy recursively."""
    for relative_name in PERSISTENT_NAMES:
        source = root / relative_name
        if not path_lexists(source):
            continue
        if relative_name == PRODUCTION_CONFIG_NAME:
            try:
                source_mode = source.lstat().st_mode
            except OSError as exception:
                raise InstallerError(
                    f"Unable to inspect persistent configuration {source}: {exception}"
                ) from exception
            if not stat.S_ISREG(source_mode):
                raise InstallerError(
                    f"Persistent configuration must be a regular file: {source}"
                )
        else:
            validate_backup_tree(source, runner)


def validate_mutable_installation_paths(
    root: pathlib.Path, runner: CommandRunner
) -> None:
    """Reject unsafe objects below every service-writable directory before removal."""
    validate_persistent_source(root, runner)
    logs_path = root / "logs"
    if path_lexists(logs_path):
        validate_backup_tree(logs_path, runner)


def path_is_within(path: pathlib.Path, parent: pathlib.Path) -> bool:
    """Return whether path is equal to or located below parent."""
    return path == parent or parent in path.parents


def reject_existing_symlink_components(path: pathlib.Path, description: str) -> None:
    """Reject an existing link anywhere in an absolute path used by root commands."""
    current = pathlib.Path(path.anchor)
    for path_part in path.parts[1:]:
        current /= path_part
        try:
            current_status = current.lstat()
        except FileNotFoundError:
            continue
        except OSError as exception:
            raise InstallerError(
                f"Unable to inspect {description} {current}: {exception}"
            ) from exception
        if stat.S_ISLNK(current_status.st_mode):
            raise InstallerError(f"{description} contains a symbolic link: {current}")


def validate_backup_destination(destination_root: pathlib.Path) -> pathlib.Path:
    """Resolve a backup root outside every installer-managed or transaction directory."""
    try:
        resolved = destination_root.expanduser().resolve(strict=False)
    except OSError as exception:
        raise InstallerError(
            f"Unable to resolve backup destination {destination_root}: {exception}"
        ) from exception
    reject_existing_symlink_components(
        destination_root.expanduser().absolute(), "Backup path"
    )
    managed_paths = (
        INSTALL_ROOT.resolve(strict=False),
        INSTALLER_STATE_ROOT.resolve(strict=False),
        require_temp_root().resolve(strict=False),
        INSTALL_ROOT.with_name(f"{INSTALL_ROOT.name}.next").resolve(strict=False),
        INSTALL_ROOT.with_name(f"{INSTALL_ROOT.name}.previous").resolve(strict=False),
        INSTALL_ROOT.with_name(f"{INSTALL_ROOT.name}.uninstalling").resolve(
            strict=False
        ),
    )
    if any(
        path_is_within(resolved, managed) or path_is_within(managed, resolved)
        for managed in managed_paths
    ):
        raise InstallerError(
            f"Backup destination {resolved} must be outside installer-managed paths."
        )
    if resolved.exists() and not resolved.is_dir():
        raise InstallerError(f"Backup destination is not a directory: {resolved}")
    return resolved


def require_root_controlled_ancestor(path: pathlib.Path) -> None:
    """Require the nearest existing backup ancestor to be exclusively root controlled."""
    ancestor = path
    while not ancestor.exists():
        if ancestor == ancestor.parent:
            raise InstallerError(f"Unable to find an existing ancestor for {path}.")
        ancestor = ancestor.parent
    try:
        ancestor_status = ancestor.stat()
    except OSError as exception:
        raise InstallerError(
            f"Unable to inspect backup destination ancestor {ancestor}: {exception}"
        ) from exception
    if ancestor_status.st_uid != 0 or ancestor_status.st_mode & 0o022:
        raise InstallerError(
            f"Backup destination ancestor must be root-owned and not group/other writable: {ancestor}"
        )


def backup_manifest_value(version: Version, contents: Sequence[str]) -> dict[str, Any]:
    """Create a portable backup manifest with the source application version."""
    return {
        "schemaVersion": BACKUP_SCHEMA_VERSION,
        "appVersion": str(version),
        "createdAt": dt.datetime.now(dt.timezone.utc).isoformat(),
        "contents": list(contents),
    }


def create_backup(
    destination_root: pathlib.Path,
    runner: CommandRunner,
    *,
    restart_service: bool,
) -> pathlib.Path:
    """Create a consistent versioned backup of UzonMail persistent files."""
    version = get_installed_version()
    resolved_root = validate_backup_destination(destination_root)
    timestamp = dt.datetime.now(dt.timezone.utc).strftime("%Y%m%dT%H%M%S%fZ")
    backup_path = resolved_root / f"uzonmail-backup-{version}-{timestamp}"
    staging_path = INSTALLER_STATE_ROOT / f"backup-staging-{timestamp}"
    require_root_controlled_ancestor(resolved_root)
    runner.run(
        ["install", "-d", "-m", "0700", "-o", "root", "-g", "root", resolved_root],
        description=f"Create root-only backup destination {resolved_root}",
    )
    was_active = is_service_active()
    copied: list[str] = []
    backup_completed = False
    try:
        if was_active:
            stop_service(runner)
        validate_persistent_source(INSTALL_ROOT, runner)
        missing_names = [
            name for name in PERSISTENT_NAMES if not path_lexists(INSTALL_ROOT / name)
        ]
        if missing_names:
            raise InstallerError(
                "Cannot create a restorable backup because persistent paths are missing: "
                + ", ".join(missing_names)
            )
        runner.run(
            [
                "install",
                "-d",
                "-m",
                "0700",
                "-o",
                "root",
                "-g",
                "root",
                staging_path,
            ],
            description=f"Create protected backup staging directory {staging_path}",
        )
        for relative_name in PERSISTENT_NAMES:
            source = INSTALL_ROOT / relative_name
            if not source.exists():
                continue
            runner.run(
                ["cp", "-a", "--one-file-system", source, staging_path / relative_name],
                description=f"Back up {source}",
            )
            copied.append(relative_name)
        manifest_source = write_temporary_json(
            BACKUP_MANIFEST_NAME, backup_manifest_value(version, copied)
        )
        runner.run(
            [
                "install",
                "-m",
                "0600",
                "-o",
                "root",
                "-g",
                "root",
                manifest_source,
                staging_path / BACKUP_MANIFEST_NAME,
            ],
            description=f"Write staged backup manifest {staging_path / BACKUP_MANIFEST_NAME}",
        )
        runner.run(
            ["chmod", "-R", "u=rwX,go=", staging_path],
            description=f"Restrict staged backup access under {staging_path}",
        )
        runner.run(
            ["cp", "-a", "-T", staging_path, backup_path],
            description=f"Publish root-owned backup at {backup_path}",
        )
        runner.run(
            ["chown", "-R", "--no-dereference", "root:root", backup_path],
            description=f"Protect backup ownership under {backup_path}",
        )
        runner.run(
            ["chmod", "-R", "u=rwX,go=", backup_path],
            description=f"Restrict backup access under {backup_path}",
        )
        backup_completed = True
    finally:
        if not backup_completed:
            try:
                runner.run(
                    ["rm", "-rf", "--one-file-system", backup_path],
                    description=f"Remove incomplete backup {backup_path}",
                )
            except InstallerError as cleanup_error:
                print(
                    f"Warning: incomplete backup remains at {backup_path}: {cleanup_error}",
                    file=sys.stderr,
                )
        try:
            runner.run(
                ["rm", "-rf", "--one-file-system", staging_path],
                description=f"Remove backup staging directory {staging_path}",
            )
        except InstallerError as cleanup_error:
            print(
                f"Warning: backup staging files remain at {staging_path}: {cleanup_error}",
                file=sys.stderr,
            )
        if was_active and (restart_service or not backup_completed):
            start_service(runner)
    print(f"Backup created: {backup_path}")
    return backup_path


def read_backup_manifest(backup_path: pathlib.Path) -> BackupManifest:
    """Validate and parse a complete backup manifest."""
    validate_backup_tree(backup_path)
    manifest_path = backup_path / BACKUP_MANIFEST_NAME
    try:
        value = json.loads(
            read_limited_file(
                manifest_path, MAX_MANIFEST_BYTES, "backup manifest"
            ).decode("utf-8")
        )
        if value.get("schemaVersion") != BACKUP_SCHEMA_VERSION:
            raise InstallerError(f"Unsupported backup schema in {manifest_path}.")
        contents = value.get("contents")
        if (
            not isinstance(contents, list)
            or any(name not in PERSISTENT_NAMES for name in contents)
            or len(contents) != len(set(contents))
            or set(contents) != set(PERSISTENT_NAMES)
        ):
            raise InstallerError(f"Invalid backup contents in {manifest_path}.")
        created_at = dt.datetime.fromisoformat(str(value["createdAt"]))
        if created_at.tzinfo is None:
            raise InstallerError(
                f"Backup creation time must include a timezone: {manifest_path}"
            )
        for relative_name in contents:
            content_path = backup_path / relative_name
            if not content_path.exists():
                raise InstallerError(f"Backup content is missing: {content_path}")
        return BackupManifest(
            version=Version.parse(str(value["appVersion"]), "backup version"),
            created_at=created_at,
            contents=tuple(contents),
        )
    except (
        KeyError,
        TypeError,
        ValueError,
        json.JSONDecodeError,
    ) as exception:
        raise InstallerError(
            f"Invalid backup manifest {manifest_path}: {exception}"
        ) from exception


def backup_is_compatible(
    backup_version: Version,
    installed_version: Version,
    minimum_compatible_version: Version | None,
    *,
    has_install_state: bool,
) -> bool:
    """Check whether a backup can be safely restored into the installed version."""
    if backup_version > installed_version:
        return False
    if not has_install_state:
        return backup_version == installed_version
    return (
        minimum_compatible_version is None
        or backup_version >= minimum_compatible_version
    )


def find_backup_candidates(source: pathlib.Path) -> list[pathlib.Path]:
    """Find valid backup directories at an exact path or beneath a parent directory."""
    reject_existing_symlink_components(source.expanduser().absolute(), "Backup source")
    resolved = source.expanduser().resolve()
    if (resolved / BACKUP_MANIFEST_NAME).is_file():
        return [resolved]
    if not resolved.is_dir():
        return []
    return sorted(
        (
            child
            for child in resolved.iterdir()
            if child.is_dir() and (child / BACKUP_MANIFEST_NAME).is_file()
        ),
        reverse=True,
    )


def select_backup(
    source: pathlib.Path,
    installed_version: Version,
    minimum_compatible_version: Version | None,
    *,
    has_install_state: bool,
    quiet: bool,
) -> pathlib.Path:
    """Select an explicitly requested or latest compatible backup."""
    compatible: list[tuple[dt.datetime, pathlib.Path]] = []
    for candidate in find_backup_candidates(source):
        try:
            manifest = read_backup_manifest(candidate)
        except InstallerError as exception:
            print(f"Skipping invalid backup {candidate}: {exception}")
            continue
        if backup_is_compatible(
            manifest.version,
            installed_version,
            minimum_compatible_version,
            has_install_state=has_install_state,
        ):
            compatible.append((manifest.created_at, candidate))
    if not compatible:
        raise InstallerError(f"No compatible backup was found under {source}.")
    compatible.sort(reverse=True)
    if quiet or len(compatible) == 1:
        return compatible[0][1]
    print("Compatible backups:")
    for index, (_, path) in enumerate(compatible, start=1):
        manifest = read_backup_manifest(path)
        print(
            f"  {index}. {path} (version {manifest.version}, {manifest.created_at.isoformat()})"
        )
    while True:
        selected = prompt_value("Select backup number", "1")
        if selected.isdigit() and 1 <= int(selected) <= len(compatible):
            return compatible[int(selected) - 1][1]
        print("Invalid backup selection.")


def copy_backup_contents(
    backup_path: pathlib.Path,
    manifest: BackupManifest,
    runner: CommandRunner,
    destination_root: pathlib.Path = INSTALL_ROOT,
) -> None:
    """Replace installed persistent files with validated backup contents."""
    validate_backup_tree(backup_path, runner)
    for relative_name in PERSISTENT_NAMES:
        destination = destination_root / relative_name
        if destination.exists():
            runner.run(
                ["rm", "-rf", "--one-file-system", destination],
                description=f"Remove current {destination} before restore",
            )
    for relative_name in manifest.contents:
        source = backup_path / relative_name
        destination = destination_root / relative_name
        runner.run(
            ["cp", "-a", source, destination],
            description=f"Restore {destination} from {source}",
        )
    validate_persistent_source(destination_root, runner)
    set_installation_permissions(destination_root, runner)


def stage_backup_for_restore(
    backup_path: pathlib.Path,
    expected_manifest: BackupManifest,
    runner: CommandRunner,
) -> tuple[pathlib.Path, pathlib.Path]:
    """Copy a validated backup into a root-only staging tree before restore."""
    timestamp = dt.datetime.now(dt.timezone.utc).strftime("%Y%m%dT%H%M%S%fZ")
    staging_parent = INSTALLER_STATE_ROOT / f"restore-source-{timestamp}"
    staged_backup = staging_parent / "backup"
    try:
        runner.run(
            [
                "install",
                "-d",
                "-m",
                "0700",
                "-o",
                "root",
                "-g",
                "root",
                staging_parent,
            ],
            description=f"Create restore source staging directory {staging_parent}",
        )
        runner.run(
            ["cp", "-a", "--one-file-system", "-T", backup_path, staged_backup],
            description=f"Stage backup {backup_path} for restore",
        )
        staged_manifest = read_backup_manifest(staged_backup)
        if staged_manifest != expected_manifest:
            raise InstallerError(
                f"Backup {backup_path} changed while it was being staged. Retry the restore."
            )
        runner.run(
            ["chown", "-R", "--no-dereference", "root:root", staging_parent],
            description=f"Protect staged restore source {staging_parent}",
        )
        runner.run(
            ["chmod", "0700", staging_parent],
            description=f"Restrict staged restore source {staging_parent}",
        )
        validate_backup_tree(staged_backup, runner)
        return staged_backup, staging_parent
    except (Exception, KeyboardInterrupt):
        try:
            runner.run(
                ["rm", "-rf", "--one-file-system", staging_parent],
                description=f"Remove failed restore source staging {staging_parent}",
            )
        except InstallerError as cleanup_error:
            print(
                f"Warning: restore source staging remains at {staging_parent}: "
                f"{cleanup_error}",
                file=sys.stderr,
            )
        raise


def remove_staged_backup(staging_parent: pathlib.Path, runner: CommandRunner) -> None:
    """Best-effort remove a root-controlled restore source after its single use."""
    try:
        runner.run(
            ["rm", "-rf", "--one-file-system", staging_parent],
            description=f"Remove staged restore source {staging_parent}",
        )
    except InstallerError as cleanup_error:
        print(
            f"Warning: staged restore source remains at {staging_parent}: {cleanup_error}",
            file=sys.stderr,
        )


def restore_backup(
    source: pathlib.Path,
    runner: CommandRunner,
    *,
    start_after_restore: bool | None = None,
    installed_version: Version | None = None,
    minimum_compatible_version: Version | None = None,
    has_install_state: bool | None = None,
) -> pathlib.Path:
    """Restore a compatible backup transactionally with automatic rollback."""
    target_version = installed_version or get_installed_version()
    if installed_version is None:
        state = require_install_state(target_version)
        validate_service_user()
        minimum_compatible_version = state.minimum_compatible_version
        state_exists = True
    else:
        state_exists = bool(has_install_state)
    backup_path = select_backup(
        source,
        target_version,
        minimum_compatible_version,
        has_install_state=state_exists,
        quiet=runner.quiet,
    )
    backup_manifest = read_backup_manifest(backup_path)
    confirm_change(
        f"Restore backup {backup_path} (version {backup_manifest.version}) into UzonMail {target_version}",
        runner.quiet,
    )
    was_active = is_service_active()
    should_start = was_active if start_after_restore is None else start_after_restore
    rollback_path = INSTALL_ROOT.with_name(f"{INSTALL_ROOT.name}.restore-rollback")
    if path_lexists(rollback_path):
        raise InstallerError(
            f"Restore rollback directory {rollback_path} already exists. Inspect and remove it "
            "only after confirming that the installed persistent files are intact."
        )
    staged_backup, source_staging_parent = stage_backup_for_restore(
        backup_path, backup_manifest, runner
    )
    persistent_changes_started = False
    rollback_names: list[str] = []
    try:
        if was_active:
            stop_service(runner)
        validate_persistent_source(INSTALL_ROOT, runner)
        runner.run(
            ["install", "-d", "-m", "0700", "-o", "root", "-g", "root", rollback_path],
            description=f"Create restore rollback directory {rollback_path}",
        )
        for relative_name in PERSISTENT_NAMES:
            current = INSTALL_ROOT / relative_name
            if current.exists():
                runner.run(
                    ["cp", "-a", current, rollback_path / relative_name],
                    description=f"Stage rollback copy of {current}",
                )
                rollback_names.append(relative_name)
        persistent_changes_started = True
        copy_backup_contents(staged_backup, backup_manifest, runner)
        if should_start:
            start_service(runner)
    except (Exception, KeyboardInterrupt) as original_error:
        rollback_errors: list[str] = []
        if persistent_changes_started:
            print(
                "Restore failed; restoring the previous persistent files.",
                file=sys.stderr,
            )
            try:
                stop_service(runner)
            except InstallerError as rollback_error:
                rollback_errors.append(str(rollback_error))
            for relative_name in PERSISTENT_NAMES:
                current = INSTALL_ROOT / relative_name
                try:
                    runner.run(
                        ["rm", "-rf", "--one-file-system", current],
                        description=f"Remove failed restore content {current}",
                    )
                except InstallerError as rollback_error:
                    rollback_errors.append(str(rollback_error))
            for relative_name in rollback_names:
                current = INSTALL_ROOT / relative_name
                rollback = rollback_path / relative_name
                try:
                    runner.run(
                        ["mv", rollback, current],
                        description=f"Roll back {current}",
                    )
                except InstallerError as rollback_error:
                    rollback_errors.append(str(rollback_error))
            try:
                set_installation_permissions(INSTALL_ROOT, runner)
            except InstallerError as rollback_error:
                rollback_errors.append(str(rollback_error))
        if was_active:
            try:
                start_service(runner)
            except InstallerError as rollback_error:
                rollback_errors.append(str(rollback_error))
        if rollback_errors:
            remove_staged_backup(source_staging_parent, runner)
            raise InstallerError(
                f"{original_error}; rollback also failed: {'; '.join(rollback_errors)}. "
                f"Inspect {INSTALL_ROOT} and {rollback_path}."
            ) from original_error
        remove_staged_backup(source_staging_parent, runner)
        raise
    try:
        runner.run(
            ["rm", "-rf", "--one-file-system", rollback_path],
            description=f"Remove restore rollback directory {rollback_path}",
        )
    except InstallerError as cleanup_error:
        print(
            f"Warning: restore succeeded, but rollback files remain at {rollback_path}: "
            f"{cleanup_error}",
            file=sys.stderr,
        )
    remove_staged_backup(source_staging_parent, runner)
    print(f"Backup restored: {backup_path}")
    return backup_path


def install_application(runner: CommandRunner) -> None:
    """Perform a confirmed first-time UzonMail installation."""
    staging_root = INSTALL_ROOT.with_name(f"{INSTALL_ROOT.name}.installing")
    service_user_existed = existing_service_user_is_valid()
    conflicting_paths = [
        INSTALL_ROOT,
        staging_root,
        INSTALL_ROOT.with_name(f"{INSTALL_ROOT.name}.next"),
        INSTALL_ROOT.with_name(f"{INSTALL_ROOT.name}.previous"),
        INSTALL_ROOT.with_name(f"{INSTALL_ROOT.name}.restore-rollback"),
        INSTALL_ROOT.with_name(f"{INSTALL_ROOT.name}.uninstalling"),
        INSTALL_STATE_PATH,
        INSTALLER_STATE_ROOT.with_name(f"{INSTALLER_STATE_ROOT.name}.uninstalling"),
        SERVICE_UNIT_PATH,
    ]
    if not service_user_existed:
        conflicting_paths.append(SERVICE_HOME)
    existing_paths = [path for path in conflicting_paths if path_lexists(path)]
    if existing_paths:
        raise InstallerError(
            "Installation cannot start while these managed or recovery paths exist: "
            + ", ".join(os.fspath(path) for path in existing_paths)
        )
    config = collect_install_config(runner.quiet)
    print("Reading the latest release metadata...")
    manifest = fetch_update_manifest()
    package_root = prepare_package(manifest, runner)
    production_config = create_production_config(package_root, config)
    production_source = write_temporary_json(PRODUCTION_CONFIG_NAME, production_config)
    confirm_change(
        f"Install UzonMail {manifest.version} at {INSTALL_ROOT} using these settings",
        runner.quiet,
    )
    save_cached_install_config(config)
    try:
        dotnet_path = ensure_dotnet_environment(manifest, runner)
        ensure_service_user(runner)
        state = create_install_state(manifest, dotnet_path)
        runner.run(
            ["install", "-d", "-m", "0755", "-o", "root", "-g", "root", staging_root],
            description=f"Create installation staging directory {staging_root}",
        )
        runner.run(
            ["cp", "-a", f"{package_root}/.", staging_root],
            description=f"Stage UzonMail {manifest.version}",
        )
        install_json_file(
            runner,
            production_source,
            staging_root / PRODUCTION_CONFIG_NAME,
            f"Stage production configuration at {staging_root / PRODUCTION_CONFIG_NAME}",
        )
        set_installation_permissions(staging_root, runner)
        candidates = find_backup_candidates(DEFAULT_BACKUP_ROOT)
        should_restore = bool(candidates) and (
            runner.quiet
            or ask_yes_no(
                "A backup was found. Restore the latest compatible backup?",
                default=True,
            )
        )
        if should_restore:
            backup_path = select_backup(
                DEFAULT_BACKUP_ROOT,
                manifest.version,
                manifest.minimum_compatible_version,
                has_install_state=True,
                quiet=runner.quiet,
            )
            backup_manifest = read_backup_manifest(backup_path)
            staged_backup, source_staging_parent = stage_backup_for_restore(
                backup_path, backup_manifest, runner
            )
            try:
                copy_backup_contents(
                    staged_backup, backup_manifest, runner, staging_root
                )
            finally:
                remove_staged_backup(source_staging_parent, runner)
        runner.run(
            ["mv", staging_root, INSTALL_ROOT],
            description=f"Activate installation at {INSTALL_ROOT}",
        )
        register_and_start_service(dotnet_path, runner)
        write_install_state(state, runner)
    except (Exception, KeyboardInterrupt) as original_error:
        rollback_errors: list[str] = []
        for rollback_action in (
            lambda: stop_service(runner),
            lambda: runner.run(
                ["systemctl", "disable", SERVICE_NAME],
                description=f"Disable failed {SERVICE_NAME}",
                check=False,
            ),
            lambda: runner.run(
                ["rm", "-f", SERVICE_UNIT_PATH],
                description=f"Remove failed unit {SERVICE_UNIT_PATH}",
            ),
            lambda: runner.run(
                ["systemctl", "daemon-reload"],
                description="Reload systemd after failed installation",
            ),
            lambda: runner.run(
                ["rm", "-rf", "--one-file-system", INSTALL_ROOT, staging_root],
                description="Remove failed installation files",
            ),
        ):
            try:
                rollback_action()
            except InstallerError as rollback_error:
                rollback_errors.append(str(rollback_error))
        if rollback_errors:
            raise InstallerError(
                f"{original_error}; installation rollback also failed: "
                f"{'; '.join(rollback_errors)}."
            ) from original_error
        raise
    PENDING_CONFIG_PATH.unlink(missing_ok=True)
    print(f"UzonMail {manifest.version} was installed successfully.")
    print(f"Open {config.base_url} to continue setup.")


def update_application(runner: CommandRunner) -> None:
    """Update UzonMail with a same-filesystem directory swap and rollback."""
    current_version = get_installed_version()
    previous_state = require_install_state(current_version)
    validate_service_user()
    print("Reading the latest release metadata...")
    manifest = fetch_update_manifest()
    if manifest.version == current_version:
        print(f"UzonMail {current_version} is already up to date.")
        return
    if manifest.version < current_version:
        print(
            f"Installed version {current_version} is newer than published version {manifest.version}; no downgrade was performed."
        )
        return
    if (
        manifest.minimum_compatible_version is not None
        and current_version < manifest.minimum_compatible_version
    ):
        raise InstallerError(
            f"Version {current_version} cannot update directly to {manifest.version}. "
            "Export your data, uninstall UzonMail, install the latest version, and then import the data."
        )
    confirm_change(
        f"Update UzonMail from {current_version} to {manifest.version}", runner.quiet
    )
    package_root = prepare_package(manifest, runner)
    dotnet_path = ensure_dotnet_environment(manifest, runner)
    next_state = create_install_state(manifest, dotnet_path)
    next_root = INSTALL_ROOT.with_name(f"{INSTALL_ROOT.name}.next")
    previous_root = INSTALL_ROOT.with_name(f"{INSTALL_ROOT.name}.previous")
    stale_paths = [path for path in (next_root, previous_root) if path_lexists(path)]
    if stale_paths:
        raise InstallerError(
            "An earlier update left recovery paths that must be inspected manually: "
            + ", ".join(os.fspath(path) for path in stale_paths)
        )
    runner.run(
        ["install", "-d", "-m", "0755", "-o", "root", "-g", "root", next_root],
        description=f"Create update staging directory {next_root}",
    )
    runner.run(
        ["cp", "-a", f"{package_root}/.", next_root],
        description=f"Stage UzonMail {manifest.version}",
    )
    was_active = is_service_active()
    was_enabled = is_service_enabled()
    temp_root = require_temp_root()
    unit_snapshot = temp_root / "previous-systemd-unit"
    state_snapshot = temp_root / "previous-install-state"
    had_unit = snapshot_system_file(SERVICE_UNIT_PATH, unit_snapshot)
    had_state = snapshot_system_file(INSTALL_STATE_PATH, state_snapshot)
    try:
        if was_active:
            stop_service(runner)
        validate_mutable_installation_paths(INSTALL_ROOT, runner)
        for relative_name in PERSISTENT_NAMES:
            source = INSTALL_ROOT / relative_name
            if not source.exists():
                continue
            destination = next_root / relative_name
            if destination.exists():
                runner.run(
                    ["rm", "-rf", "--one-file-system", destination],
                    description=f"Remove packaged persistent path {destination}",
                )
            runner.run(
                ["cp", "-a", source, destination],
                description=f"Preserve {source} during update",
            )
        set_installation_permissions(next_root, runner)
    except (Exception, KeyboardInterrupt) as original_error:
        rollback_errors: list[str] = []
        try:
            runner.run(
                ["rm", "-rf", "--one-file-system", next_root],
                description=f"Remove failed staging release {next_root}",
            )
        except InstallerError as rollback_error:
            rollback_errors.append(str(rollback_error))
        if was_active:
            try:
                start_service(runner)
            except InstallerError as rollback_error:
                rollback_errors.append(str(rollback_error))
        if rollback_errors:
            raise InstallerError(
                f"{original_error}; update preparation rollback also failed: "
                f"{'; '.join(rollback_errors)}."
            ) from original_error
        raise
    try:
        runner.run(
            ["mv", INSTALL_ROOT, previous_root],
            description=f"Move the current release to {previous_root}",
        )
        runner.run(
            ["mv", next_root, INSTALL_ROOT],
            description=f"Activate UzonMail {manifest.version}",
        )
        install_service_unit(dotnet_path, runner)
        if was_active:
            start_service(runner)
        write_install_state(next_state, runner)
    except (Exception, KeyboardInterrupt) as original_error:
        print("Update failed; restoring the previous release.", file=sys.stderr)
        rollback_errors: list[str] = []
        try:
            stop_service(runner)
        except InstallerError as rollback_error:
            rollback_errors.append(str(rollback_error))
        previous_release_exists = path_lexists(previous_root)
        if previous_release_exists and path_lexists(INSTALL_ROOT):
            try:
                runner.run(
                    ["rm", "-rf", "--one-file-system", INSTALL_ROOT],
                    description=f"Remove failed release {INSTALL_ROOT}",
                )
            except InstallerError as rollback_error:
                rollback_errors.append(str(rollback_error))
        if previous_release_exists and not path_lexists(INSTALL_ROOT):
            try:
                runner.run(
                    ["mv", previous_root, INSTALL_ROOT],
                    description=f"Restore previous release to {INSTALL_ROOT}",
                )
            except InstallerError as rollback_error:
                rollback_errors.append(str(rollback_error))
        if next_root.exists():
            try:
                runner.run(
                    ["rm", "-rf", "--one-file-system", next_root],
                    description=f"Remove failed staging release {next_root}",
                )
            except InstallerError as rollback_error:
                rollback_errors.append(str(rollback_error))
        for restore_action in (
            lambda: restore_system_file(
                unit_snapshot, SERVICE_UNIT_PATH, runner, existed=had_unit
            ),
            lambda: restore_system_file(
                state_snapshot, INSTALL_STATE_PATH, runner, existed=had_state
            ),
            lambda: restore_service_state(
                runner, was_enabled=was_enabled, was_active=was_active
            ),
        ):
            try:
                restore_action()
            except InstallerError as rollback_error:
                rollback_errors.append(str(rollback_error))
        if rollback_errors:
            raise InstallerError(
                f"{original_error}; update rollback also failed: {'; '.join(rollback_errors)}. "
                f"Inspect {INSTALL_ROOT}, {previous_root}, and {next_root}."
            ) from original_error
        raise
    try:
        runner.run(
            ["rm", "-rf", "--one-file-system", previous_root],
            description=f"Remove the previous release {previous_root}",
        )
    except InstallerError as cleanup_error:
        print(
            f"Warning: update succeeded, but the previous release remains at "
            f"{previous_root}: {cleanup_error}",
            file=sys.stderr,
        )
    print(f"UzonMail was updated successfully to {manifest.version}.")


def uninstall_application(runner: CommandRunner) -> None:
    """Back up optional data and remove UzonMail-owned system resources."""
    installed_version = get_installed_version()
    require_install_state(installed_version)
    validate_service_user()
    confirm_change(
        "Uninstall UzonMail and remove its service unit, installation, and installer state",
        runner.quiet,
    )
    was_active = is_service_active()
    was_enabled = is_service_enabled()
    unit_snapshot = require_temp_root() / "uninstall-systemd-unit"
    had_unit = snapshot_system_file(SERVICE_UNIT_PATH, unit_snapshot)
    uninstall_root = INSTALL_ROOT.with_name(f"{INSTALL_ROOT.name}.uninstalling")
    uninstall_state_root = INSTALLER_STATE_ROOT.with_name(
        f"{INSTALLER_STATE_ROOT.name}.uninstalling"
    )
    stale_paths = [
        path for path in (uninstall_root, uninstall_state_root) if path_lexists(path)
    ]
    if stale_paths:
        raise InstallerError(
            "An earlier uninstall left recovery paths that must be inspected manually: "
            + ", ".join(os.fspath(path) for path in stale_paths)
        )
    should_backup = (
        True
        if runner.quiet
        else ask_yes_no("Back up UzonMail data before uninstalling?", default=True)
    )
    if should_backup:
        destination = DEFAULT_BACKUP_ROOT
        if not runner.quiet:
            destination = pathlib.Path(
                prompt_value("Backup destination", os.fspath(DEFAULT_BACKUP_ROOT))
            )
        create_backup(destination, runner, restart_service=False)
    try:
        stop_service(runner)
        validate_mutable_installation_paths(INSTALL_ROOT, runner)
        runner.run(
            ["mv", INSTALL_ROOT, uninstall_root],
            description=f"Stage installation for removal at {uninstall_root}",
        )
        runner.run(
            ["mv", INSTALLER_STATE_ROOT, uninstall_state_root],
            description=f"Stage installer state for removal at {uninstall_state_root}",
        )
        runner.run(
            ["systemctl", "disable", SERVICE_NAME],
            description=f"Disable {SERVICE_NAME}",
        )
        runner.run(
            ["rm", "-f", SERVICE_UNIT_PATH],
            description=f"Remove systemd unit {SERVICE_UNIT_PATH}",
        )
        runner.run(
            ["systemctl", "daemon-reload"],
            description="Reload systemd units",
        )
    except (Exception, KeyboardInterrupt) as original_error:
        rollback_errors: list[str] = []
        if path_lexists(uninstall_state_root) and not path_lexists(
            INSTALLER_STATE_ROOT
        ):
            try:
                runner.run(
                    ["mv", uninstall_state_root, INSTALLER_STATE_ROOT],
                    description=f"Restore installer state to {INSTALLER_STATE_ROOT}",
                )
            except InstallerError as rollback_error:
                rollback_errors.append(str(rollback_error))
        if path_lexists(uninstall_root) and not path_lexists(INSTALL_ROOT):
            try:
                runner.run(
                    ["mv", uninstall_root, INSTALL_ROOT],
                    description=f"Restore installation to {INSTALL_ROOT}",
                )
            except InstallerError as rollback_error:
                rollback_errors.append(str(rollback_error))
        for restore_action in (
            lambda: restore_system_file(
                unit_snapshot, SERVICE_UNIT_PATH, runner, existed=had_unit
            ),
            lambda: restore_service_state(
                runner, was_enabled=was_enabled, was_active=was_active
            ),
        ):
            try:
                restore_action()
            except InstallerError as rollback_error:
                rollback_errors.append(str(rollback_error))
        if rollback_errors:
            raise InstallerError(
                f"{original_error}; uninstall rollback also failed: "
                f"{'; '.join(rollback_errors)}. Inspect {uninstall_root} and "
                f"{uninstall_state_root}."
            ) from original_error
        raise

    cleanup_failures: list[str] = []
    for path in (uninstall_root, uninstall_state_root):
        try:
            runner.run(
                ["rm", "-rf", "--one-file-system", path],
                description=f"Remove committed uninstall path {path}",
            )
        except InstallerError as cleanup_error:
            cleanup_failures.append(f"{path}: {cleanup_error}")
    if cleanup_failures:
        raise InstallerError(
            "UzonMail was uninstalled, but cleanup left these resources: "
            + "; ".join(cleanup_failures)
        )
    print(
        f"UzonMail was uninstalled. Backups under {DEFAULT_BACKUP_ROOT} were preserved."
    )


def parse_arguments(arguments: Sequence[str] | None = None) -> argparse.Namespace:
    """Parse the mutually exclusive installer command line."""
    parser = argparse.ArgumentParser(
        description=(
            "Install and maintain UzonMail on x64 systemd Linux. System-changing actions "
            "create a service account, application directories, and a systemd service."
        )
    )
    parser.add_argument(
        "-q",
        "--quiet",
        action="store_true",
        help="use defaults and run system-changing steps without confirmation",
    )
    actions = parser.add_mutually_exclusive_group(required=True)
    actions.add_argument(
        "--install", action="store_true", help="perform a first-time installation"
    )
    actions.add_argument(
        "--update", action="store_true", help="update to the latest compatible version"
    )
    actions.add_argument(
        "--uninstall",
        action="store_true",
        help="back up optionally and uninstall UzonMail",
    )
    actions.add_argument(
        "--backup",
        nargs="?",
        const=os.fspath(DEFAULT_BACKUP_ROOT),
        metavar="DESTINATION",
        help="back up persistent files to a versioned directory",
    )
    actions.add_argument(
        "--restore",
        nargs="?",
        const=os.fspath(DEFAULT_BACKUP_ROOT),
        metavar="SOURCE",
        help="restore an exact backup directory or the latest compatible backup under SOURCE",
    )
    actions.add_argument(
        "--version", action="store_true", help="show the installed UzonMail version"
    )
    return parser.parse_args(arguments)


def main(arguments: Sequence[str] | None = None) -> int:
    """Dispatch one installer action and convert expected failures to exit codes."""
    global TEMP_ROOT
    lock_descriptor: int | None = None
    try:
        original_arguments = list(arguments) if arguments is not None else sys.argv[1:]
        options = parse_arguments(original_arguments)
        if options.version:
            print(get_installed_version())
            return 0
        action = next(
            name
            for name in ("install", "update", "uninstall", "backup", "restore")
            if getattr(options, name) not in {False, None}
        )
        reexecute_as_root(original_arguments, options.quiet)
        validate_platform()
        TEMP_ROOT = create_private_temp_root()
        lock_descriptor = acquire_installer_lock()
        print_startup_notice(action, options.quiet)
        runner = CommandRunner(options.quiet)
        validate_managed_path_layout()
        if options.install:
            install_application(runner)
        elif options.update:
            update_application(runner)
        elif options.uninstall:
            uninstall_application(runner)
        elif options.backup is not None:
            confirm_change(
                f"Back up UzonMail data under {options.backup}", options.quiet
            )
            create_backup(pathlib.Path(options.backup), runner, restart_service=True)
        elif options.restore is not None:
            restore_backup(pathlib.Path(options.restore), runner)
        return 0
    except KeyboardInterrupt:
        print(
            "\nOperation interrupted. Saved installation input remains available for the next run.",
            file=sys.stderr,
        )
        return 130
    except OperationCancelled as exception:
        print(exception, file=sys.stderr)
        return 2
    except InstallerError as exception:
        print(f"Error: {exception}", file=sys.stderr)
        return 1
    finally:
        if lock_descriptor is not None:
            os.close(lock_descriptor)
        if TEMP_ROOT is not None:
            shutil.rmtree(TEMP_ROOT, ignore_errors=True)
            TEMP_ROOT = None


if __name__ == "__main__":
    raise SystemExit(main())
