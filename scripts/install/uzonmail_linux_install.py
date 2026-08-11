#!/usr/bin/env python3
"""Install, update, back up, restore, or uninstall UzonMail on Linux."""

from __future__ import annotations

import argparse
import dataclasses
import datetime as dt
import getpass
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
STATE_ROOT = pathlib.Path("/var/lib/uzonmail")
INSTALL_STATE_PATH = STATE_ROOT / "install-state.json"
SERVICE_UNIT_PATH = pathlib.Path("/etc/systemd/system") / SERVICE_NAME
TEMP_ROOT = pathlib.Path("/tmp/uzonmail")
TEMP_CONFIG_PATH = TEMP_ROOT / "config.json"
DEFAULT_BACKUP_ROOT = pathlib.Path("/var/uzonmail/backup")
LATEST_MANIFEST_URL = "https://uzonmail.uzoncloud.com/updates/latest.json"
DOTNET_INSTALL_URL = "https://dot.net/v1/dotnet-install.sh"
DOTNET_INSTALL_ROOT = pathlib.Path("/usr/share/dotnet")
DOTNET_COMMAND_PATH = pathlib.Path("/usr/bin/dotnet")
PACKAGE_DIRECTORY_NAME = "service-linux-x64"
SERVICE_ASSEMBLY_NAME = "UzonMailService.dll"
PRODUCTION_CONFIG_NAME = "appsettings.Production.json"
BACKUP_MANIFEST_NAME = "backup-manifest.json"
INSTALL_STATE_SCHEMA_VERSION = 1
BACKUP_SCHEMA_VERSION = 1
NETWORK_TIMEOUT_SECONDS = 30
MAX_MANIFEST_BYTES = 64 * 1024 * 1024
DEFAULT_BASE_URL = "http://localhost:22345"
DEFAULT_ADMIN_USER = "admin"
DEFAULT_ADMIN_PASSWORD = "admin1234"
DEFAULT_TOKEN_EXPIRATION_MILLISECONDS = 24 * 60 * 60 * 1000
SUPPORTED_FRAMEWORKS = {
    "Microsoft.AspNetCore.App": "aspnetcore",
    "Microsoft.NETCore.App": "dotnet",
}
PERSISTENT_NAMES = (PRODUCTION_CONFIG_NAME, "data", "public")


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
        if not 2 <= len(parts) <= 4 or any(not part.isdigit() for part in parts):
            raise InstallerError(f"Invalid {field_name}: {value!r}")
        normalized = [int(part) for part in parts]
        normalized.extend([0] * (4 - len(normalized)))
        return cls(*normalized)

    def __str__(self) -> str:
        return f"{self.major}.{self.minor}.{self.build}.{self.revision}"


@dataclasses.dataclass(frozen=True)
class UpdateManifest:
    """Contain the validated update metadata required by the Linux installer."""

    version: Version
    environment: dict[str, Version]
    package_url: str
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


class CommandRunner:
    """Execute commands consistently with sudo and per-step confirmation."""

    def __init__(self, quiet: bool) -> None:
        self.quiet = quiet
        self.is_root = getattr(os, "geteuid", lambda: -1)() == 0

    def run(
        self,
        arguments: Sequence[str | os.PathLike[str]],
        *,
        description: str,
        privileged: bool = False,
        mutation: bool = False,
        check: bool = True,
        capture_output: bool = False,
        confirm: bool = True,
    ) -> subprocess.CompletedProcess[str]:
        """Run one command after disclosing and, when required, confirming it."""
        command = [os.fspath(argument) for argument in arguments]
        if privileged and not self.is_root:
            command.insert(0, "sudo")
        if mutation and confirm:
            confirm_change(description, self.quiet)
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
    print(f"It manages {INSTALL_ROOT}, {STATE_ROOT}, and {TEMP_ROOT}.")
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


def validate_sudo_access(runner: CommandRunner) -> None:
    """Validate root or sudo access before an operation can modify the system."""
    if runner.is_root:
        return
    if shutil.which("sudo") is None:
        raise InstallerError(
            "sudo is required. Install sudo or run this script as root."
        )
    arguments = ["sudo", "-n", "-v"] if runner.quiet else ["sudo", "-v"]
    try:
        subprocess.run(arguments, check=True)
    except subprocess.CalledProcessError as exception:
        if runner.quiet:
            raise InstallerError(
                "Quiet mode requires cached sudo credentials. Run 'sudo -v' first or run as root."
            ) from exception
        raise InstallerError(
            "The current user does not have usable sudo permission."
        ) from exception


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


def load_cached_install_config() -> InstallConfig | None:
    """Load validated interrupted-install input when it is available."""
    if not TEMP_CONFIG_PATH.is_file():
        return None
    try:
        value = json.loads(TEMP_CONFIG_PATH.read_text(encoding="utf-8"))
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
    ) as exception:
        print(f"Ignoring invalid cached installer input: {exception}")
        return None


def save_cached_install_config(config: InstallConfig) -> None:
    """Persist installer input atomically with owner-only permissions."""
    TEMP_ROOT.mkdir(mode=0o700, parents=True, exist_ok=True)
    temporary_path = TEMP_CONFIG_PATH.with_suffix(".tmp")
    temporary_path.write_text(
        json.dumps(config.to_json_value(), indent=2) + "\n", encoding="utf-8"
    )
    temporary_path.chmod(0o600)
    temporary_path.replace(TEMP_CONFIG_PATH)


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
    defaults = cached or InstallConfig(
        base_url=DEFAULT_BASE_URL,
        admin_user=DEFAULT_ADMIN_USER,
        admin_password=DEFAULT_ADMIN_PASSWORD,
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
    save_cached_install_config(config)
    print("Installation settings:")
    print(f"  Base URL: {config.base_url}")
    print(f"  Administrator: {config.admin_user}")
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
    zip_url = str(value.get("zipUrl", ""))
    package_url = infer_linux_package_url(zip_url, version)
    minimum_value = value.get("minCompatibleVersion")
    minimum_version = (
        Version.parse(str(minimum_value), "minimum compatible version")
        if minimum_value is not None
        else None
    )
    return UpdateManifest(version, environment, package_url, minimum_version)


def infer_linux_package_url(zip_url: str, version: Version) -> str:
    """Infer the Linux service archive beside the manifest's desktop archive."""
    parsed = urllib.parse.urlsplit(zip_url)
    if parsed.scheme != "https" or not parsed.netloc:
        raise InstallerError("The update package URL must use HTTPS.")
    parent = parsed.path.rsplit("/", 1)[0]
    filename = f"uzonmail-service-linux-x64-{version}.zip"
    return urllib.parse.urlunsplit(
        (parsed.scheme, parsed.netloc, f"{parent}/{filename}", parsed.query, "")
    )


def download_file(url: str, destination: pathlib.Path, quiet: bool) -> None:
    """Download a file atomically while reporting bounded progress."""
    confirm_change(f"Download {url} to {destination}", quiet)
    destination.parent.mkdir(mode=0o700, parents=True, exist_ok=True)
    partial_path = destination.with_suffix(destination.suffix + ".part")
    request = urllib.request.Request(
        url, headers={"User-Agent": "UzonMail-Linux-Installer/1"}
    )
    try:
        with urllib.request.urlopen(
            request, timeout=NETWORK_TIMEOUT_SECONDS
        ) as response:
            total_text = response.headers.get("Content-Length")
            total = int(total_text) if total_text and total_text.isdigit() else None
            downloaded = 0
            next_report = 10
            with partial_path.open("wb") as output:
                while True:
                    chunk = response.read(1024 * 1024)
                    if not chunk:
                        break
                    output.write(chunk)
                    downloaded += len(chunk)
                    if total:
                        percentage = downloaded * 100 // total
                        if percentage >= next_report:
                            print(f"   Downloaded {min(percentage, 100)}%")
                            next_report += 10
        partial_path.replace(destination)
    except (OSError, urllib.error.URLError) as exception:
        partial_path.unlink(missing_ok=True)
        raise InstallerError(f"Unable to download {url}: {exception}") from exception


def safe_extract_package(
    archive_path: pathlib.Path, destination: pathlib.Path, quiet: bool
) -> pathlib.Path:
    """Extract a release archive after rejecting unsafe ZIP entries."""
    confirm_change(f"Extract {archive_path} to {destination}", quiet)
    if destination.exists():
        shutil.rmtree(destination)
    destination.mkdir(mode=0o700, parents=True)
    try:
        with zipfile.ZipFile(archive_path) as archive:
            package_entries: list[zipfile.ZipInfo] = []
            for entry in archive.infolist():
                normalized = entry.filename.replace("\\", "/")
                path_parts = pathlib.PurePosixPath(normalized).parts
                file_mode = entry.external_attr >> 16
                if (
                    normalized.startswith("/")
                    or ".." in path_parts
                    or stat.S_ISLNK(file_mode)
                ):
                    raise InstallerError(f"Unsafe archive entry: {entry.filename}")
                if path_parts and path_parts[0] == PACKAGE_DIRECTORY_NAME:
                    package_entries.append(entry)
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
        content = assembly_path.read_bytes()
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
        OSError,
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


def read_installed_runtimes() -> dict[str, list[Version]]:
    """Read installed shared frameworks from dotnet without changing the system."""
    dotnet = shutil.which("dotnet")
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


def ensure_dotnet_environment(manifest: UpdateManifest, runner: CommandRunner) -> None:
    """Install missing .NET shared frameworks with Microsoft's official installer."""
    installed = read_installed_runtimes()
    missing = {
        name: version
        for name, version in manifest.environment.items()
        if not runtime_requirement_is_met(installed.get(name, ()), version)
    }
    if not missing:
        print("Required .NET runtimes are already installed.")
        return
    installer_path = TEMP_ROOT / "dotnet-install.sh"
    download_file(DOTNET_INSTALL_URL, installer_path, runner.quiet)
    installer_path.chmod(0o700)
    ordered_names = sorted(missing, key=lambda name: name != "Microsoft.AspNetCore.App")
    for framework_name in ordered_names:
        installed = read_installed_runtimes()
        required = missing[framework_name]
        if runtime_requirement_is_met(installed.get(framework_name, ()), required):
            continue
        runner.run(
            [
                "bash",
                installer_path,
                "--runtime",
                SUPPORTED_FRAMEWORKS[framework_name],
                "--version",
                str(required),
                "--install-dir",
                DOTNET_INSTALL_ROOT,
                "--no-path",
            ],
            description=f"Install {framework_name} {required}",
            privileged=True,
            mutation=True,
        )
    runner.run(
        ["ln", "-sfn", DOTNET_INSTALL_ROOT / "dotnet", DOTNET_COMMAND_PATH],
        description=f"Link dotnet at {DOTNET_COMMAND_PATH}",
        privileged=True,
        mutation=True,
    )
    installed = read_installed_runtimes()
    unresolved = [
        f"{name} {version}"
        for name, version in manifest.environment.items()
        if not runtime_requirement_is_met(installed.get(name, ()), version)
    ]
    if unresolved:
        raise InstallerError(
            f"Required runtimes are still missing: {', '.join(unresolved)}"
        )


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
    TEMP_ROOT.mkdir(mode=0o700, parents=True, exist_ok=True)
    path = TEMP_ROOT / filename
    path.write_text(json.dumps(value, indent=2) + "\n", encoding="utf-8")
    path.chmod(mode)
    return path


def service_unit_content() -> str:
    """Return the systemd unit for the dedicated UzonMail service account."""
    install_root = INSTALL_ROOT.as_posix()
    state_root = STATE_ROOT.as_posix()
    dotnet_path = DOTNET_COMMAND_PATH.as_posix()
    return f"""[Unit]
Description=UzonMail Service
Wants=network-online.target
After=network-online.target

[Service]
Type=simple
User={SERVICE_USER}
Group={SERVICE_USER}
WorkingDirectory={install_root}
ExecStart={dotnet_path} {install_root}/{SERVICE_ASSEMBLY_NAME}
Restart=always
RestartSec=10
SyslogIdentifier=uzon-mail
UMask=0027
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=HOME={state_root}

[Install]
WantedBy=multi-user.target
"""


def ensure_service_user(runner: CommandRunner) -> None:
    """Create the dedicated no-login service account when it does not exist."""
    result = subprocess.run(["id", "-u", SERVICE_USER], capture_output=True, text=True)
    if result.returncode == 0:
        return
    nologin = shutil.which("nologin") or shutil.which("false") or "/bin/false"
    runner.run(
        [
            "useradd",
            "--system",
            "--user-group",
            "--home-dir",
            STATE_ROOT,
            "--create-home",
            "--shell",
            nologin,
            SERVICE_USER,
        ],
        description=f"Create system account '{SERVICE_USER}' with home {STATE_ROOT}",
        privileged=True,
        mutation=True,
    )


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
        privileged=True,
        mutation=True,
    )


def install_state_value(manifest: UpdateManifest) -> dict[str, Any]:
    """Create persistent compatibility metadata for backup restoration."""
    return {
        "schemaVersion": INSTALL_STATE_SCHEMA_VERSION,
        "version": str(manifest.version),
        "minCompatibleVersion": (
            str(manifest.minimum_compatible_version)
            if manifest.minimum_compatible_version is not None
            else None
        ),
        "installedAt": dt.datetime.now(dt.timezone.utc).isoformat(),
    }


def write_install_state(manifest: UpdateManifest, runner: CommandRunner) -> None:
    """Persist the installed version and restore compatibility boundary."""
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
            STATE_ROOT,
        ],
        description=f"Create installer state directory {STATE_ROOT}",
        privileged=True,
        mutation=True,
    )
    temporary = write_temporary_json(
        "install-state.json", install_state_value(manifest), 0o644
    )
    install_json_file(
        runner,
        temporary,
        INSTALL_STATE_PATH,
        f"Record installation state at {INSTALL_STATE_PATH}",
        mode="0644",
        group="root",
    )


def read_install_state() -> tuple[Version, Version | None] | None:
    """Read restore compatibility state without using it as the version authority."""
    if not INSTALL_STATE_PATH.is_file():
        return None
    try:
        value = json.loads(INSTALL_STATE_PATH.read_text(encoding="utf-8"))
        if value.get("schemaVersion") != INSTALL_STATE_SCHEMA_VERSION:
            return None
        minimum = value.get("minCompatibleVersion")
        return (
            Version.parse(str(value["version"]), "installed state version"),
            Version.parse(str(minimum), "installed minimum compatible version")
            if minimum
            else None,
        )
    except (OSError, KeyError, TypeError, json.JSONDecodeError, InstallerError):
        return None


def prepare_package(manifest: UpdateManifest, runner: CommandRunner) -> pathlib.Path:
    """Download, safely extract, and version-check the latest Linux package."""
    archive = TEMP_ROOT / f"uzonmail-service-linux-x64-{manifest.version}.zip"
    extraction_root = TEMP_ROOT / "extracted" / str(manifest.version)
    download_file(manifest.package_url, archive, runner.quiet)
    package_root = safe_extract_package(archive, extraction_root, runner.quiet)
    package_version = read_dotnet_assembly_version(package_root / SERVICE_ASSEMBLY_NAME)
    if package_version != manifest.version:
        raise InstallerError(
            f"Package version {package_version} does not match manifest version {manifest.version}."
        )
    return package_root


def set_installation_permissions(root: pathlib.Path, runner: CommandRunner) -> None:
    """Grant the service account access only to runtime-writable application paths."""
    for relative_name in ("data", "public", "logs", "users"):
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
            privileged=True,
            mutation=True,
        )
        runner.run(
            ["chown", "-R", f"{SERVICE_USER}:{SERVICE_USER}", path],
            description=f"Assign {path} to the service account",
            privileged=True,
            mutation=True,
        )
    frontend_config = root / "wwwroot/app.config.json"
    if frontend_config.exists():
        runner.run(
            ["chown", f"{SERVICE_USER}:{SERVICE_USER}", frontend_config],
            description=f"Allow the service to update {frontend_config}",
            privileged=True,
            mutation=True,
        )
        runner.run(
            ["chmod", "0640", frontend_config],
            description=f"Restrict permissions on {frontend_config}",
            privileged=True,
            mutation=True,
        )


def register_and_start_service(runner: CommandRunner) -> None:
    """Install, enable, start, and verify the UzonMail systemd unit."""
    unit_source = TEMP_ROOT / SERVICE_NAME
    unit_source.write_text(service_unit_content(), encoding="utf-8")
    unit_source.chmod(0o644)
    runner.run(
        [
            "install",
            "-m",
            "0644",
            "-o",
            "root",
            "-g",
            "root",
            unit_source,
            SERVICE_UNIT_PATH,
        ],
        description=f"Register systemd unit {SERVICE_UNIT_PATH}",
        privileged=True,
        mutation=True,
    )
    runner.run(
        ["systemctl", "daemon-reload"],
        description="Reload systemd units",
        privileged=True,
        mutation=True,
    )
    runner.run(
        ["systemctl", "enable", SERVICE_NAME],
        description=f"Enable {SERVICE_NAME} at boot",
        privileged=True,
        mutation=True,
    )
    start_service(runner)


def is_service_active() -> bool:
    """Return whether the UzonMail systemd unit is currently active."""
    result = subprocess.run(
        ["systemctl", "is-active", "--quiet", SERVICE_NAME], capture_output=True
    )
    return result.returncode == 0


def start_service(runner: CommandRunner, *, confirm: bool = True) -> None:
    """Start UzonMail and verify that it remains active after initialization."""
    runner.run(
        ["systemctl", "start", SERVICE_NAME],
        description=f"Start {SERVICE_NAME}",
        privileged=True,
        mutation=True,
        confirm=confirm,
    )
    time.sleep(2)
    if not is_service_active():
        raise InstallerError(
            f"{SERVICE_NAME} did not remain active. Check 'journalctl -u {SERVICE_NAME}'."
        )


def stop_service(runner: CommandRunner, *, confirm: bool = True) -> None:
    """Stop UzonMail when it is active."""
    if is_service_active():
        runner.run(
            ["systemctl", "stop", SERVICE_NAME],
            description=f"Stop {SERVICE_NAME}",
            privileged=True,
            mutation=True,
            confirm=confirm,
        )


def invoking_user() -> tuple[str, str]:
    """Return the original invoking user's account and primary group names."""
    import grp
    import pwd

    username = os.environ.get("SUDO_USER")
    if not username or username == "root":
        username = pwd.getpwuid(os.getuid()).pw_name
    account = pwd.getpwnam(username)
    return username, grp.getgrgid(account.pw_gid).gr_name


def validate_backup_tree(path: pathlib.Path) -> None:
    """Reject backup trees containing symbolic links or non-regular objects."""
    if not path.is_dir() or path.is_symlink():
        raise InstallerError(f"Backup directory is invalid: {path}")
    for root, directories, files in os.walk(path, followlinks=False):
        for name in [*directories, *files]:
            candidate = pathlib.Path(root) / name
            if candidate.is_symlink():
                raise InstallerError(f"Backup contains a symbolic link: {candidate}")


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
    resolved_root = destination_root.expanduser().resolve()
    install_resolved = INSTALL_ROOT.resolve()
    if resolved_root == install_resolved or install_resolved in resolved_root.parents:
        raise InstallerError(
            "The backup destination cannot be inside the installation directory."
        )
    timestamp = dt.datetime.now(dt.timezone.utc).strftime("%Y%m%dT%H%M%S%fZ")
    backup_path = resolved_root / f"uzonmail-backup-{version}-{timestamp}"
    username, group_name = invoking_user()
    was_active = is_service_active()
    if was_active:
        stop_service(runner)
    copied: list[str] = []
    backup_completed = False
    try:
        runner.run(
            [
                "install",
                "-d",
                "-m",
                "0700",
                "-o",
                username,
                "-g",
                group_name,
                backup_path,
            ],
            description=f"Create backup directory {backup_path}",
            privileged=True,
            mutation=True,
        )
        for relative_name in PERSISTENT_NAMES:
            source = INSTALL_ROOT / relative_name
            if not source.exists():
                continue
            runner.run(
                ["cp", "-a", source, backup_path / relative_name],
                description=f"Back up {source}",
                privileged=True,
                mutation=True,
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
                username,
                "-g",
                group_name,
                manifest_source,
                backup_path / BACKUP_MANIFEST_NAME,
            ],
            description=f"Write backup manifest {backup_path / BACKUP_MANIFEST_NAME}",
            privileged=True,
            mutation=True,
        )
        runner.run(
            ["chown", "-R", f"{username}:{group_name}", backup_path],
            description=f"Assign backup ownership to {username}",
            privileged=True,
            mutation=True,
        )
        backup_completed = True
    finally:
        if was_active and (restart_service or not backup_completed):
            start_service(runner, confirm=False)
    print(f"Backup created: {backup_path}")
    return backup_path


def read_backup_manifest(backup_path: pathlib.Path) -> tuple[Version, dt.datetime]:
    """Validate a backup manifest and return its source version and creation time."""
    validate_backup_tree(backup_path)
    manifest_path = backup_path / BACKUP_MANIFEST_NAME
    try:
        value = json.loads(manifest_path.read_text(encoding="utf-8"))
        if value.get("schemaVersion") != BACKUP_SCHEMA_VERSION:
            raise InstallerError(f"Unsupported backup schema in {manifest_path}.")
        contents = value.get("contents")
        if not isinstance(contents, list) or any(
            name not in PERSISTENT_NAMES for name in contents
        ):
            raise InstallerError(f"Invalid backup contents in {manifest_path}.")
        created_at = dt.datetime.fromisoformat(str(value["createdAt"]))
        if created_at.tzinfo is None:
            raise InstallerError(
                f"Backup creation time must include a timezone: {manifest_path}"
            )
        return Version.parse(str(value["appVersion"]), "backup version"), created_at
    except (
        OSError,
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
            version, created_at = read_backup_manifest(candidate)
        except InstallerError as exception:
            print(f"Skipping invalid backup {candidate}: {exception}")
            continue
        if backup_is_compatible(
            version,
            installed_version,
            minimum_compatible_version,
            has_install_state=has_install_state,
        ):
            compatible.append((created_at, candidate))
    if not compatible:
        raise InstallerError(f"No compatible backup was found under {source}.")
    compatible.sort(reverse=True)
    if quiet or len(compatible) == 1:
        return compatible[0][1]
    print("Compatible backups:")
    for index, (_, path) in enumerate(compatible, start=1):
        version, created_at = read_backup_manifest(path)
        print(f"  {index}. {path} (version {version}, {created_at.isoformat()})")
    while True:
        selected = prompt_value("Select backup number", "1")
        if selected.isdigit() and 1 <= int(selected) <= len(compatible):
            return compatible[int(selected) - 1][1]
        print("Invalid backup selection.")


def copy_backup_contents(backup_path: pathlib.Path, runner: CommandRunner) -> None:
    """Replace installed persistent files with validated backup contents."""
    validate_backup_tree(backup_path)
    value = json.loads((backup_path / BACKUP_MANIFEST_NAME).read_text(encoding="utf-8"))
    for relative_name in value["contents"]:
        source = backup_path / relative_name
        destination = INSTALL_ROOT / relative_name
        if not source.exists():
            raise InstallerError(f"Backup content is missing: {source}")
        if destination.exists():
            runner.run(
                ["rm", "-rf", destination],
                description=f"Remove current {destination} before restore",
                privileged=True,
                mutation=True,
            )
        runner.run(
            ["cp", "-a", source, destination],
            description=f"Restore {destination} from {source}",
            privileged=True,
            mutation=True,
        )
    set_installation_permissions(INSTALL_ROOT, runner)
    production_config = INSTALL_ROOT / PRODUCTION_CONFIG_NAME
    if production_config.exists():
        runner.run(
            ["chown", f"root:{SERVICE_USER}", production_config],
            description=f"Assign secure ownership to {production_config}",
            privileged=True,
            mutation=True,
        )
        runner.run(
            ["chmod", "0640", production_config],
            description=f"Restrict permissions on {production_config}",
            privileged=True,
            mutation=True,
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
    state = read_install_state()
    matching_state = state if state is not None and state[0] == target_version else None
    state_exists = (
        matching_state is not None if has_install_state is None else has_install_state
    )
    if minimum_compatible_version is None and matching_state is not None:
        minimum_compatible_version = matching_state[1]
    backup_path = select_backup(
        source,
        target_version,
        minimum_compatible_version,
        has_install_state=state_exists,
        quiet=runner.quiet,
    )
    backup_version, _ = read_backup_manifest(backup_path)
    confirm_change(
        f"Restore backup {backup_path} (version {backup_version}) into UzonMail {target_version}",
        runner.quiet,
    )
    was_active = is_service_active()
    should_start = was_active if start_after_restore is None else start_after_restore
    rollback_path = pathlib.Path(f"/var/uzonmail/.restore-rollback-{os.getpid()}")
    persistent_changes_started = False
    if was_active:
        stop_service(runner)
    try:
        runner.run(
            ["rm", "-rf", rollback_path],
            description=f"Clear restore rollback directory {rollback_path}",
            privileged=True,
            mutation=True,
        )
        runner.run(
            ["install", "-d", "-m", "0700", "-o", "root", "-g", "root", rollback_path],
            description=f"Create restore rollback directory {rollback_path}",
            privileged=True,
            mutation=True,
        )
        for relative_name in PERSISTENT_NAMES:
            current = INSTALL_ROOT / relative_name
            if current.exists():
                runner.run(
                    ["cp", "-a", current, rollback_path / relative_name],
                    description=f"Stage rollback copy of {current}",
                    privileged=True,
                    mutation=True,
                )
        persistent_changes_started = True
        copy_backup_contents(backup_path, runner)
        if should_start:
            start_service(runner)
        runner.run(
            ["rm", "-rf", rollback_path],
            description=f"Remove restore rollback directory {rollback_path}",
            privileged=True,
            mutation=True,
        )
    except Exception:
        if persistent_changes_started:
            print(
                "Restore failed; restoring the previous persistent files.",
                file=sys.stderr,
            )
            for relative_name in PERSISTENT_NAMES:
                current = INSTALL_ROOT / relative_name
                rollback = rollback_path / relative_name
                if current.exists():
                    runner.run(
                        ["rm", "-rf", current],
                        description=f"Remove failed restore content {current}",
                        privileged=True,
                        mutation=True,
                        confirm=False,
                    )
                if rollback.exists():
                    runner.run(
                        ["mv", rollback, current],
                        description=f"Roll back {current}",
                        privileged=True,
                        mutation=True,
                        confirm=False,
                    )
        if should_start:
            start_service(runner, confirm=False)
        raise
    print(f"Backup restored: {backup_path}")
    return backup_path


def install_application(runner: CommandRunner) -> None:
    """Perform a confirmed first-time UzonMail installation."""
    if INSTALL_ROOT.exists() and any(INSTALL_ROOT.iterdir()):
        raise InstallerError(
            f"Installation directory {INSTALL_ROOT} is not empty. "
            "Use --update, --backup, or --uninstall, or inspect the partial installation manually."
        )
    config = collect_install_config(runner.quiet)
    confirm_change(
        "Proceed with the UzonMail installation using these settings", runner.quiet
    )
    print("Reading the latest release metadata...")
    manifest = fetch_update_manifest()
    ensure_dotnet_environment(manifest, runner)
    package_root = prepare_package(manifest, runner)
    production_config = create_production_config(package_root, config)
    production_source = write_temporary_json(PRODUCTION_CONFIG_NAME, production_config)
    ensure_service_user(runner)
    runner.run(
        ["install", "-d", "-m", "0755", "-o", "root", "-g", "root", INSTALL_ROOT],
        description=f"Create installation directory {INSTALL_ROOT}",
        privileged=True,
        mutation=True,
    )
    runner.run(
        ["cp", "-a", f"{package_root}/.", INSTALL_ROOT],
        description=f"Install UzonMail {manifest.version} into {INSTALL_ROOT}",
        privileged=True,
        mutation=True,
    )
    install_json_file(
        runner,
        production_source,
        INSTALL_ROOT / PRODUCTION_CONFIG_NAME,
        f"Install production configuration at {INSTALL_ROOT / PRODUCTION_CONFIG_NAME}",
    )
    set_installation_permissions(INSTALL_ROOT, runner)
    write_install_state(manifest, runner)
    candidates = find_backup_candidates(DEFAULT_BACKUP_ROOT)
    should_restore = bool(candidates) and (
        runner.quiet
        or ask_yes_no(
            "A backup was found. Restore the latest compatible backup?", default=True
        )
    )
    if should_restore:
        restore_backup(
            DEFAULT_BACKUP_ROOT,
            runner,
            start_after_restore=False,
            installed_version=manifest.version,
            minimum_compatible_version=manifest.minimum_compatible_version,
            has_install_state=True,
        )
    register_and_start_service(runner)
    shutil.rmtree(TEMP_ROOT, ignore_errors=True)
    print(f"UzonMail {manifest.version} was installed successfully.")
    print(f"Open {config.base_url} to continue setup.")


def update_application(runner: CommandRunner) -> None:
    """Update UzonMail with a same-filesystem directory swap and rollback."""
    current_version = get_installed_version()
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
    ensure_dotnet_environment(manifest, runner)
    package_root = prepare_package(manifest, runner)
    next_root = INSTALL_ROOT.with_name(f"{INSTALL_ROOT.name}.next")
    previous_root = INSTALL_ROOT.with_name(f"{INSTALL_ROOT.name}.previous")
    for path in (next_root, previous_root):
        runner.run(
            ["rm", "-rf", path],
            description=f"Clear update staging directory {path}",
            privileged=True,
            mutation=True,
        )
    runner.run(
        ["install", "-d", "-m", "0755", "-o", "root", "-g", "root", next_root],
        description=f"Create update staging directory {next_root}",
        privileged=True,
        mutation=True,
    )
    runner.run(
        ["cp", "-a", f"{package_root}/.", next_root],
        description=f"Stage UzonMail {manifest.version}",
        privileged=True,
        mutation=True,
    )
    for relative_name in PERSISTENT_NAMES:
        source = INSTALL_ROOT / relative_name
        if not source.exists():
            continue
        destination = next_root / relative_name
        if destination.exists():
            runner.run(
                ["rm", "-rf", destination],
                description=f"Remove packaged persistent path {destination}",
                privileged=True,
                mutation=True,
            )
        runner.run(
            ["cp", "-a", source, destination],
            description=f"Preserve {source} during update",
            privileged=True,
            mutation=True,
        )
    set_installation_permissions(next_root, runner)
    was_active = is_service_active()
    if was_active:
        stop_service(runner)
    switched = False
    try:
        runner.run(
            ["mv", INSTALL_ROOT, previous_root],
            description=f"Move the current release to {previous_root}",
            privileged=True,
            mutation=True,
        )
        runner.run(
            ["mv", next_root, INSTALL_ROOT],
            description=f"Activate UzonMail {manifest.version}",
            privileged=True,
            mutation=True,
        )
        switched = True
        start_service(runner)
        write_install_state(manifest, runner)
        runner.run(
            ["rm", "-rf", previous_root],
            description=f"Remove the previous release {previous_root}",
            privileged=True,
            mutation=True,
        )
    except Exception:
        if switched:
            print("Update failed; restoring the previous release.", file=sys.stderr)
            stop_service(runner, confirm=False)
            if INSTALL_ROOT.exists():
                runner.run(
                    ["rm", "-rf", INSTALL_ROOT],
                    description=f"Remove failed release {INSTALL_ROOT}",
                    privileged=True,
                    mutation=True,
                    confirm=False,
                )
            if previous_root.exists():
                runner.run(
                    ["mv", previous_root, INSTALL_ROOT],
                    description=f"Restore previous release to {INSTALL_ROOT}",
                    privileged=True,
                    mutation=True,
                    confirm=False,
                )
        if was_active and not is_service_active():
            start_service(runner, confirm=False)
        raise
    shutil.rmtree(TEMP_ROOT, ignore_errors=True)
    print(f"UzonMail was updated successfully to {manifest.version}.")


def uninstall_application(runner: CommandRunner) -> None:
    """Back up optional data and remove UzonMail-owned system resources."""
    get_installed_version()
    confirm_change(
        "Uninstall UzonMail, remove its service account, service unit, installation, and temporary files",
        runner.quiet,
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
    stop_service(runner)
    runner.run(
        ["systemctl", "disable", SERVICE_NAME],
        description=f"Disable {SERVICE_NAME}",
        privileged=True,
        mutation=True,
        check=False,
    )
    runner.run(
        ["rm", "-f", SERVICE_UNIT_PATH],
        description=f"Remove systemd unit {SERVICE_UNIT_PATH}",
        privileged=True,
        mutation=True,
    )
    runner.run(
        ["systemctl", "daemon-reload"],
        description="Reload systemd units",
        privileged=True,
        mutation=True,
    )
    runner.run(
        ["rm", "-rf", INSTALL_ROOT],
        description=f"Remove installation directory {INSTALL_ROOT}",
        privileged=True,
        mutation=True,
    )
    if subprocess.run(["id", "-u", SERVICE_USER], capture_output=True).returncode == 0:
        runner.run(
            ["userdel", "--remove", SERVICE_USER],
            description=f"Remove system account '{SERVICE_USER}' and {STATE_ROOT}",
            privileged=True,
            mutation=True,
            check=False,
        )
    runner.run(
        ["rm", "-rf", STATE_ROOT, TEMP_ROOT],
        description=f"Remove installer state and temporary files ({STATE_ROOT}, {TEMP_ROOT})",
        privileged=True,
        mutation=True,
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
    try:
        options = parse_arguments(arguments)
        if options.version:
            print(get_installed_version())
            return 0
        action = next(
            name
            for name in ("install", "update", "uninstall", "backup", "restore")
            if getattr(options, name) not in {False, None}
        )
        print_startup_notice(action, options.quiet)
        validate_platform()
        runner = CommandRunner(options.quiet)
        validate_sudo_access(runner)
        if options.install:
            install_application(runner)
        elif options.update:
            update_application(runner)
        elif options.uninstall:
            uninstall_application(runner)
        elif options.backup is not None:
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


if __name__ == "__main__":
    raise SystemExit(main())
