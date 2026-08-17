"""Tests for the standalone UzonMail Linux installer."""

from __future__ import annotations

import importlib.util
import json
import os
import pathlib
import stat
import sys
import tempfile
import types
import unittest
import zipfile
from unittest import mock


REPOSITORY_ROOT = pathlib.Path(__file__).resolve().parents[2]
INSTALLER_PATH = REPOSITORY_ROOT / "scripts/install/uzonmail_linux_install.py"
SPEC = importlib.util.spec_from_file_location("uzonmail_linux_install", INSTALLER_PATH)
if SPEC is None or SPEC.loader is None:
    raise RuntimeError(f"Unable to load installer module from {INSTALLER_PATH}")
installer = importlib.util.module_from_spec(SPEC)
sys.modules[SPEC.name] = installer
SPEC.loader.exec_module(installer)


class VersionTests(unittest.TestCase):
    """Verify numeric version parsing and comparison."""

    def test_parse_normalizes_missing_revision(self) -> None:
        self.assertEqual(installer.Version.parse("10.0"), installer.Version(10, 0, 0, 0))
        self.assertEqual(str(installer.Version.parse("0.23.5.0")), "0.23.5.0")

    def test_parse_rejects_non_numeric_or_oversized_versions(self) -> None:
        for value in ("1", "1.2.3.4.5", "1.2-beta", ""):
            with self.subTest(value=value), self.assertRaises(installer.InstallerError):
                installer.Version.parse(value)

    def test_runtime_compatibility_requires_same_major_and_minimum_version(self) -> None:
        required = installer.Version.parse("10.0.0")
        self.assertTrue(
            installer.runtime_requirement_is_met(
                [installer.Version.parse("10.0.10")], required
            )
        )
        self.assertFalse(
            installer.runtime_requirement_is_met(
                [installer.Version.parse("9.0.20"), installer.Version.parse("11.0.0")], required
            )
        )
        self.assertEqual(required.to_runtime_string(), "10.0.0")


class CommandLineTests(unittest.TestCase):
    """Verify the public mutually exclusive command-line contract."""

    def test_backup_and_restore_accept_optional_paths(self) -> None:
        backup = installer.parse_arguments(["--quiet", "--backup", "/srv/backups"])
        restore = installer.parse_arguments(["--restore"])
        self.assertTrue(backup.quiet)
        self.assertEqual(backup.backup, "/srv/backups")
        self.assertEqual(restore.restore, str(installer.DEFAULT_BACKUP_ROOT))

    def test_actions_are_mutually_exclusive(self) -> None:
        with self.assertRaises(SystemExit):
            installer.parse_arguments(["--install", "--update"])

    def test_no_action_is_rejected(self) -> None:
        with self.assertRaises(SystemExit):
            installer.parse_arguments([])

    def test_non_root_operation_reexecutes_once_with_sudo(self) -> None:
        with (
            mock.patch.object(installer.os, "geteuid", return_value=1000, create=True),
            mock.patch.object(installer.shutil, "which", return_value="/usr/bin/sudo"),
            mock.patch.object(installer.os, "execvp", side_effect=OSError("exec stopped")) as execvp,
            self.assertRaisesRegex(installer.InstallerError, "restart"),
        ):
            installer.reexecute_as_root(["--quiet", "--backup"], quiet=True)
        command = execvp.call_args.args[1]
        self.assertEqual(command[:2], ["sudo", "-n"])
        self.assertEqual(command[-2:], ["--quiet", "--backup"])


class ManifestTests(unittest.TestCase):
    """Verify update metadata validation without reading dependency entries."""

    def test_fetch_manifest_reads_required_fields(self) -> None:
        manifest_json = json.dumps(
            {
                "name": "UzonMail",
                "version": "0.23.5.0",
                "env": {
                    "Microsoft.AspNetCore.App": "10.0.0",
                    "Microsoft.NETCore.App": "10.0.0",
                },
                "dependencies": {"ignored/file.dll": "large-value-is-ignored"},
                "zipUrl": "https://example.test/files/desktop.zip",
                "artifacts": {
                    "linux-x64": {
                        "url": "https://example.test/files/linux.zip",
                        "sha256": "a" * 64,
                    }
                },
                "minCompatibleVersion": "0.20.0.0",
            }
        ).encode()
        response = mock.MagicMock()
        response.__enter__.return_value.read.return_value = manifest_json
        with mock.patch.object(installer.urllib.request, "urlopen", return_value=response):
            manifest = installer.fetch_update_manifest()
        self.assertEqual(manifest.version, installer.Version.parse("0.23.5.0"))
        self.assertEqual(
            manifest.minimum_compatible_version, installer.Version.parse("0.20.0.0")
        )
        self.assertEqual(manifest.package_url, "https://example.test/files/linux.zip")
        self.assertEqual(manifest.package_sha256, "a" * 64)

    def test_fetch_manifest_requires_linux_artifact(self) -> None:
        manifest_json = json.dumps(
            {
                "name": "UzonMail",
                "version": "1.0.0.0",
                "env": {"Microsoft.NETCore.App": "10.0.0"},
            }
        ).encode()
        response = mock.MagicMock()
        response.__enter__.return_value.read.return_value = manifest_json
        with (
            mock.patch.object(installer.urllib.request, "urlopen", return_value=response),
            self.assertRaisesRegex(installer.InstallerError, "artifacts.linux-x64"),
        ):
            installer.fetch_update_manifest()

    def test_fetch_manifest_rejects_unknown_framework(self) -> None:
        manifest_json = json.dumps(
            {
                "name": "UzonMail",
                "version": "1.0.0.0",
                "env": {"Unknown.Framework": "1.0.0"},
                "zipUrl": "https://example.test/desktop.zip",
            }
        ).encode()
        response = mock.MagicMock()
        response.__enter__.return_value.read.return_value = manifest_json
        with mock.patch.object(installer.urllib.request, "urlopen", return_value=response):
            with self.assertRaisesRegex(installer.InstallerError, "Unsupported .NET framework"):
                installer.fetch_update_manifest()


class PackageTests(unittest.TestCase):
    """Verify release archive validation and extraction."""

    def test_extracts_only_service_directory_and_allows_release_root_files(self) -> None:
        with tempfile.TemporaryDirectory() as temporary_directory:
            root = pathlib.Path(temporary_directory)
            archive_path = root / "package.zip"
            with zipfile.ZipFile(archive_path, "w") as archive:
                archive.writestr("install.sh", "legacy release helper")
                archive.writestr("service-linux-x64/UzonMailService.dll", b"managed-assembly")
                archive.writestr("service-linux-x64/data/example.txt", "content")
            extracted = installer.safe_extract_package(archive_path, root / "out", quiet=True)
            self.assertTrue((extracted / "UzonMailService.dll").is_file())
            self.assertFalse((root / "out/install.sh").exists())

    def test_rejects_path_traversal(self) -> None:
        with tempfile.TemporaryDirectory() as temporary_directory:
            root = pathlib.Path(temporary_directory)
            archive_path = root / "package.zip"
            with zipfile.ZipFile(archive_path, "w") as archive:
                archive.writestr("service-linux-x64/UzonMailService.dll", b"assembly")
                archive.writestr("service-linux-x64/../../escaped", "bad")
            with self.assertRaisesRegex(installer.InstallerError, "Unsafe archive entry"):
                installer.safe_extract_package(archive_path, root / "out", quiet=True)

    def test_rejects_symbolic_links(self) -> None:
        with tempfile.TemporaryDirectory() as temporary_directory:
            root = pathlib.Path(temporary_directory)
            archive_path = root / "package.zip"
            link = zipfile.ZipInfo("service-linux-x64/link")
            link.create_system = 3
            link.external_attr = (stat.S_IFLNK | 0o777) << 16
            with zipfile.ZipFile(archive_path, "w") as archive:
                archive.writestr("service-linux-x64/UzonMailService.dll", b"assembly")
                archive.writestr(link, "../../target")
            with self.assertRaisesRegex(installer.InstallerError, "Unsafe archive entry"):
                installer.safe_extract_package(archive_path, root / "out", quiet=True)

    def test_rejects_special_files(self) -> None:
        with tempfile.TemporaryDirectory() as temporary_directory:
            root = pathlib.Path(temporary_directory)
            archive_path = root / "package.zip"
            fifo = zipfile.ZipInfo("service-linux-x64/pipe")
            fifo.create_system = 3
            fifo.external_attr = (stat.S_IFIFO | 0o600) << 16
            with zipfile.ZipFile(archive_path, "w") as archive:
                archive.writestr("service-linux-x64/UzonMailService.dll", b"assembly")
                archive.writestr(fifo, b"")
            with self.assertRaisesRegex(installer.InstallerError, "Unsafe archive entry"):
                installer.safe_extract_package(archive_path, root / "out", quiet=True)

    def test_download_rejects_sha256_mismatch_before_publish(self) -> None:
        with tempfile.TemporaryDirectory() as temporary_directory:
            root = pathlib.Path(temporary_directory)
            destination = root / "package.zip"
            response = mock.MagicMock()
            response.__enter__.return_value.headers = {}
            response.__enter__.return_value.read.side_effect = [b"package", b""]
            with (
                mock.patch.object(installer, "TEMP_ROOT", root),
                mock.patch.object(installer.urllib.request, "urlopen", return_value=response),
                self.assertRaisesRegex(installer.InstallerError, "SHA-256 mismatch"),
            ):
                installer.download_file(
                    "https://example.test/package.zip",
                    destination,
                    quiet=True,
                    expected_sha256="0" * 64,
                )
            self.assertFalse(destination.exists())

    def test_dotnet_installer_signature_requires_expected_validsig(self) -> None:
        with tempfile.TemporaryDirectory() as temporary_directory:
            root = pathlib.Path(temporary_directory)
            runner = mock.MagicMock()
            runner.run.side_effect = [
                mock.MagicMock(stdout=""),
                mock.MagicMock(
                    stdout=f"fpr:::::::::{installer.DOTNET_INSTALL_KEY_FINGERPRINT}:\n"
                ),
                mock.MagicMock(stdout="[GNUPG:] VALIDSIG BADFINGERPRINT 2026-01-01\n"),
            ]
            with (
                mock.patch.object(installer, "TEMP_ROOT", root),
                mock.patch.object(installer.shutil, "which", return_value="/usr/bin/gpg"),
                self.assertRaisesRegex(installer.InstallerError, "expected valid signature"),
            ):
                installer.verify_dotnet_install_script(
                    root / "dotnet-install.sh",
                    root / "dotnet-install.sig",
                    root / "dotnet-install.asc",
                    runner,
                )

    def test_dotnet_installer_signature_accepts_pinned_primary_key(self) -> None:
        with tempfile.TemporaryDirectory() as temporary_directory:
            root = pathlib.Path(temporary_directory)
            fingerprint = installer.DOTNET_INSTALL_KEY_FINGERPRINT
            runner = mock.MagicMock()
            runner.run.side_effect = [
                mock.MagicMock(stdout=""),
                mock.MagicMock(stdout=f"fpr:::::::::{fingerprint}:\n"),
                mock.MagicMock(stdout=f"[GNUPG:] VALIDSIG SUBKEY 2026 0 0 0 0 0 0 {fingerprint}\n"),
            ]
            with (
                mock.patch.object(installer, "TEMP_ROOT", root),
                mock.patch.object(installer.shutil, "which", return_value="/usr/bin/gpg"),
            ):
                installer.verify_dotnet_install_script(
                    root / "dotnet-install.sh",
                    root / "dotnet-install.sig",
                    root / "dotnet-install.asc",
                    runner,
                )

    def test_dotnet_installer_signature_requires_gpg(self) -> None:
        with (
            mock.patch.object(installer.shutil, "which", return_value=None),
            self.assertRaisesRegex(installer.InstallerError, "gpg is required"),
        ):
            installer.verify_dotnet_install_script(
                pathlib.Path("dotnet-install.sh"),
                pathlib.Path("dotnet-install.sig"),
                pathlib.Path("dotnet-install.asc"),
                mock.MagicMock(),
            )


class ConfigurationTests(unittest.TestCase):
    """Verify secure cached input and production override generation."""

    def test_quiet_install_generates_random_admin_password(self) -> None:
        with (
            mock.patch.object(installer, "load_cached_install_config", return_value=None),
            mock.patch.object(installer, "save_cached_install_config") as save_config,
            mock.patch.object(
                installer.secrets,
                "token_urlsafe",
                return_value="generated-random-password",
            ),
            mock.patch("builtins.print"),
        ):
            config = installer.collect_install_config(quiet=True)
        self.assertEqual(config.admin_password, "generated-random-password")
        save_config.assert_not_called()

    def test_json_comment_stripping_preserves_comment_markers_in_strings(self) -> None:
        content = '{"url":"https://example.test/a//b",/* comment */"value":1// line\n}'
        self.assertEqual(
            json.loads(installer.strip_json_comments(content)),
            {"url": "https://example.test/a//b", "value": 1},
        )

    def test_production_config_merges_cors_and_generates_required_sections(self) -> None:
        with tempfile.TemporaryDirectory() as temporary_directory:
            root = pathlib.Path(temporary_directory)
            (root / "appsettings.json").write_text(
                '{"Cors":["https://desktop.uzonmail.com"]}', encoding="utf-8"
            )
            config = installer.InstallConfig(
                base_url="https://mail.example.test:8443/base",
                admin_user="owner",
                admin_password="secret",
                encryption_key="key",
                encryption_iv="iv",
                token_secret="token",
            )
            result = installer.create_production_config(root, config)
        self.assertEqual(
            result["Cors"],
            ["https://desktop.uzonmail.com", "https://mail.example.test:8443"],
        )
        self.assertEqual(result["User"]["AdminUser"]["UserId"], "owner")
        self.assertEqual(result["TokenParams"]["Issuer"], config.base_url)

    def test_cached_config_is_owner_only_and_round_trips(self) -> None:
        with tempfile.TemporaryDirectory() as temporary_directory:
            root = pathlib.Path(temporary_directory)
            config_path = root / "config.json"
            config = installer.InstallConfig(
                base_url="http://localhost:22345",
                admin_user="admin",
                admin_password="password",
                encryption_key="key",
                encryption_iv="iv",
                token_secret="token",
            )
            with (
                mock.patch.object(installer, "TEMP_ROOT", root),
                mock.patch.object(installer, "INSTALLER_STATE_ROOT", root),
                mock.patch.object(installer, "PENDING_CONFIG_PATH", config_path),
            ):
                installer.save_cached_install_config(config)
                loaded = installer.load_cached_install_config()
            self.assertEqual(loaded, config)
            if os.name != "nt":
                self.assertEqual(config_path.stat().st_mode & 0o777, 0o600)

    def test_temporary_subdirectory_rejects_symbolic_link_when_supported(self) -> None:
        with tempfile.TemporaryDirectory() as temporary_directory:
            root = pathlib.Path(temporary_directory)
            linked_directory = root / "linked"
            try:
                linked_directory.symlink_to(root / "outside", target_is_directory=True)
            except OSError:
                self.skipTest("Symbolic links are unavailable for this test user")
            with (
                mock.patch.object(installer, "TEMP_ROOT", root),
                self.assertRaisesRegex(installer.InstallerError, "link or non-directory"),
            ):
                installer.ensure_private_temp_directory(linked_directory / "child")


class InstallStateTests(unittest.TestCase):
    """Verify final installer state parsing and ownership metadata."""

    def test_stored_dotnet_path_does_not_need_to_still_exist(self) -> None:
        with tempfile.TemporaryDirectory() as temporary_directory:
            state_path = pathlib.Path(temporary_directory) / "install-state.json"
            state_path.write_text(
                json.dumps(
                    {
                        "schemaVersion": installer.INSTALL_STATE_SCHEMA_VERSION,
                        "version": "1.0.0.0",
                        "minCompatibleVersion": None,
                        "dotnetPath": "/removed/dotnet",
                        "installedAt": "2026-01-01T00:00:00+00:00",
                    }
                ),
                encoding="utf-8",
            )
            with mock.patch.object(installer, "INSTALL_STATE_PATH", state_path):
                state = installer.read_install_state()
        self.assertIsNotNone(state)
        self.assertEqual(state.dotnet_path, pathlib.Path("/removed/dotnet"))


class BackupTests(unittest.TestCase):
    """Verify backup manifests, selection, and compatibility boundaries."""

    def create_backup_fixture(
        self, root: pathlib.Path, name: str, version: str, created_at: str
    ) -> pathlib.Path:
        backup = root / name
        backup.mkdir()
        (backup / installer.BACKUP_MANIFEST_NAME).write_text(
            json.dumps(
                {
                    "schemaVersion": installer.BACKUP_SCHEMA_VERSION,
                    "appVersion": version,
                    "createdAt": created_at,
                    "contents": list(installer.PERSISTENT_NAMES),
                }
            ),
            encoding="utf-8",
        )
        (backup / "data").mkdir()
        (backup / installer.PRODUCTION_CONFIG_NAME).write_text("{}", encoding="utf-8")
        return backup

    def test_compatibility_uses_installed_interval(self) -> None:
        installed = installer.Version.parse("0.23.5.0")
        minimum = installer.Version.parse("0.20.0.0")
        self.assertTrue(
            installer.backup_is_compatible(
                installer.Version.parse("0.22.0.0"),
                installed,
                minimum,
                has_install_state=True,
            )
        )
        self.assertFalse(
            installer.backup_is_compatible(
                installer.Version.parse("0.19.0.0"),
                installed,
                minimum,
                has_install_state=True,
            )
        )
        self.assertFalse(
            installer.backup_is_compatible(
                installer.Version.parse("0.24.0.0"),
                installed,
                minimum,
                has_install_state=True,
            )
        )

    def test_missing_install_state_requires_exact_version(self) -> None:
        installed = installer.Version.parse("1.2.0.0")
        self.assertTrue(
            installer.backup_is_compatible(
                installed, installed, None, has_install_state=False
            )
        )
        self.assertFalse(
            installer.backup_is_compatible(
                installer.Version.parse("1.1.0.0"),
                installed,
                None,
                has_install_state=False,
            )
        )

    def test_quiet_selection_uses_latest_compatible_backup(self) -> None:
        with tempfile.TemporaryDirectory() as temporary_directory:
            root = pathlib.Path(temporary_directory)
            self.create_backup_fixture(
                root, "older", "0.22.0.0", "2026-01-01T00:00:00+00:00"
            )
            newest = self.create_backup_fixture(
                root, "newer", "0.23.0.0", "2026-02-01T00:00:00+00:00"
            )
            selected = installer.select_backup(
                root,
                installer.Version.parse("0.23.5.0"),
                installer.Version.parse("0.20.0.0"),
                has_install_state=True,
                quiet=True,
            )
        self.assertEqual(selected, newest)

    def test_backup_tree_rejects_symlinks_when_supported(self) -> None:
        with tempfile.TemporaryDirectory() as temporary_directory:
            root = pathlib.Path(temporary_directory)
            backup = self.create_backup_fixture(
                root, "backup", "1.0.0.0", "2026-01-01T00:00:00+00:00"
            )
            link = backup / "data/link"
            try:
                link.symlink_to(backup / "outside")
            except OSError:
                self.skipTest("Symbolic links are unavailable for this test user")
            with self.assertRaisesRegex(installer.InstallerError, "link or special file"):
                installer.validate_backup_tree(backup)

    def test_manifest_rejects_duplicate_contents(self) -> None:
        with tempfile.TemporaryDirectory() as temporary_directory:
            backup = pathlib.Path(temporary_directory)
            (backup / "data").mkdir()
            (backup / installer.BACKUP_MANIFEST_NAME).write_text(
                json.dumps(
                    {
                        "schemaVersion": installer.BACKUP_SCHEMA_VERSION,
                        "appVersion": "1.0.0.0",
                        "createdAt": "2026-01-01T00:00:00+00:00",
                        "contents": ["data", "data"],
                    }
                ),
                encoding="utf-8",
            )
            with self.assertRaisesRegex(installer.InstallerError, "Invalid backup contents"):
                installer.read_backup_manifest(backup)


class ServiceAndAssemblyTests(unittest.TestCase):
    """Verify the generated unit and managed assembly version reader."""

    @unittest.skipIf(os.name == "nt", "POSIX ownership is unavailable on Windows")
    def test_rejects_dotnet_host_below_user_writable_directory(self) -> None:
        with tempfile.TemporaryDirectory() as temporary_directory:
            host_path = pathlib.Path(temporary_directory) / "dotnet"
            host_path.write_text("#!/bin/sh\n", encoding="utf-8")
            host_path.chmod(0o755)
            with self.assertRaisesRegex(
                installer.InstallerError, "root-owned and not writable"
            ):
                installer.normalize_dotnet_path(host_path)

    def test_rejects_privileged_existing_service_account(self) -> None:
        account = types.SimpleNamespace(
            pw_uid=0,
            pw_gid=1001,
            pw_dir=os.fspath(installer.SERVICE_HOME),
            pw_shell="/usr/sbin/nologin",
        )
        primary_group = types.SimpleNamespace(
            gr_gid=1001,
            gr_name=installer.SERVICE_USER,
            gr_mem=[],
        )
        fake_pwd = types.SimpleNamespace(getpwnam=mock.Mock(return_value=account))
        fake_grp = types.SimpleNamespace(
            getgrgid=mock.Mock(return_value=primary_group),
            getgrall=mock.Mock(return_value=[primary_group]),
            getgrnam=mock.Mock(side_effect=KeyError),
        )
        with (
            mock.patch.dict(sys.modules, {"pwd": fake_pwd, "grp": fake_grp}),
            self.assertRaisesRegex(installer.InstallerError, "must be unprivileged"),
        ):
            installer.existing_service_user_is_valid()

    def test_service_unit_uses_dedicated_account_and_production_environment(self) -> None:
        dotnet_path = pathlib.Path("/opt/uzonmail-dotnet/dotnet")
        with mock.patch.object(installer, "normalize_dotnet_path", return_value=dotnet_path):
            unit = installer.service_unit_content(dotnet_path)
        self.assertIn("User=uzonmail", unit)
        self.assertIn("Group=uzonmail", unit)
        self.assertIn("ASPNETCORE_ENVIRONMENT=Production", unit)
        self.assertIn("Environment=HOME=/var/www/uzonmail/data", unit)
        self.assertIn("ProtectSystem=strict", unit)
        self.assertIn(
            "ReadWritePaths=/var/www/uzonmail/data /var/www/uzonmail/logs "
            "-/var/www/uzonmail/wwwroot/app.config.json",
            unit,
        )
        self.assertIn(
            "/opt/uzonmail-dotnet/dotnet /var/www/uzonmail/UzonMailService.dll",
            unit,
        )
        self.assertNotIn("/usr/bin/dotnet", unit)

    def test_permissions_do_not_create_users_directory(self) -> None:
        runner = mock.MagicMock()
        with tempfile.TemporaryDirectory() as temporary_directory:
            root = pathlib.Path(temporary_directory)
            (root / "wwwroot").mkdir()
            (root / "wwwroot/app.config.json").write_text("{}", encoding="utf-8")
            (root / installer.PRODUCTION_CONFIG_NAME).write_text("{}", encoding="utf-8")
            installer.set_installation_permissions(root, runner)
        command_text = "\n".join(
            " ".join(os.fspath(argument) for argument in call.args[0])
            for call in runner.run.call_args_list
        )
        self.assertNotIn("users", command_text)
        for directory_name in installer.WRITABLE_DIRECTORY_NAMES:
            self.assertIn(directory_name, command_text)

    def test_backup_permissions_are_owner_only(self) -> None:
        runner = mock.MagicMock(quiet=True)
        runner.run.return_value = mock.MagicMock(returncode=0, stdout="42:d\x00")
        with tempfile.TemporaryDirectory() as temporary_directory:
            root = pathlib.Path(temporary_directory)
            install_root = root / "installation"
            backup_root = root / "backups"
            temporary_root = root / "temporary"
            install_root.mkdir()
            backup_root.mkdir()
            temporary_root.mkdir()
            (install_root / installer.PRODUCTION_CONFIG_NAME).write_text(
                "{}", encoding="utf-8"
            )
            (install_root / "data").mkdir()
            with (
                mock.patch.object(installer, "INSTALL_ROOT", install_root),
                mock.patch.object(installer, "TEMP_ROOT", temporary_root),
                mock.patch.object(
                    installer,
                    "get_installed_version",
                    return_value=installer.Version(1, 0, 0, 0),
                ),
                mock.patch.object(installer, "require_root_controlled_ancestor"),
                mock.patch.object(installer, "is_service_active", return_value=False),
            ):
                installer.create_backup(backup_root, runner, restart_service=True)
        self.assertTrue(
            any(
                call.args[0][:3] == ["chmod", "-R", "u=rwX,go="]
                for call in runner.run.call_args_list
                if len(call.args[0]) >= 3
            )
        )

    def test_persistent_tree_uses_mount_safe_validation(self) -> None:
        runner = mock.MagicMock()
        runner.run.return_value = mock.MagicMock(stdout="42:d\x0042:f\x00")
        installer.validate_backup_tree(pathlib.Path("/protected/data"), runner)
        arguments = runner.run.call_args.args[0]
        self.assertEqual(arguments[:3], ["find", pathlib.Path("/protected/data"), "-xdev"])
        self.assertNotIn("privileged", runner.run.call_args.kwargs)

    def test_version_reader_rejects_non_pe_files(self) -> None:
        with tempfile.TemporaryDirectory() as temporary_directory:
            path = pathlib.Path(temporary_directory) / "not-an-assembly.dll"
            path.write_bytes(b"not a PE file")
            with self.assertRaises(installer.InstallerError):
                installer.read_dotnet_assembly_version(path)

    def test_version_reader_matches_local_linux_build_when_available(self) -> None:
        assembly = REPOSITORY_ROOT / "build/service-linux-x64/UzonMailService.dll"
        if not assembly.is_file():
            self.skipTest("Local Linux build artifact is unavailable")
        self.assertEqual(
            installer.read_dotnet_assembly_version(assembly),
            installer.Version.parse("0.23.5.0"),
        )


class TransactionRecoveryTests(unittest.TestCase):
    """Verify cancellation cannot leave service state or persistent files damaged."""

    def test_failed_uninstall_backup_restarts_previously_active_service(self) -> None:
        runner = mock.MagicMock(quiet=True)
        def run_command(arguments, **_kwargs):
            if arguments[0] == "find":
                return mock.MagicMock(stdout="42:d\x00")
            if arguments[0] == "install" and "backup-staging-" in os.fspath(arguments[-1]):
                raise installer.OperationCancelled("cancelled")
            return mock.MagicMock(returncode=0)

        runner.run.side_effect = run_command
        with tempfile.TemporaryDirectory() as temporary_directory:
            root = pathlib.Path(temporary_directory)
            install_root = root / "installation"
            temporary_root = root / "installer-temp"
            backup_root = root / "backups"
            install_root.mkdir()
            (install_root / installer.PRODUCTION_CONFIG_NAME).write_text(
                "{}", encoding="utf-8"
            )
            (install_root / "data").mkdir()
            backup_root.mkdir()
            with (
                mock.patch.object(
                    installer, "get_installed_version", return_value=installer.Version(1, 0, 0, 0)
                ),
                mock.patch.object(installer, "is_service_active", return_value=True),
                mock.patch.object(installer, "stop_service") as stop_service,
                mock.patch.object(installer, "start_service") as start_service,
                mock.patch.object(installer, "require_root_controlled_ancestor"),
                mock.patch.object(installer, "INSTALL_ROOT", install_root),
                mock.patch.object(installer, "TEMP_ROOT", temporary_root),
            ):
                with self.assertRaises(installer.OperationCancelled):
                    installer.create_backup(
                        backup_root, runner, restart_service=False
                    )
        stop_service.assert_called_once_with(runner)
        start_service.assert_called_once_with(runner)

    def test_restore_cancelled_before_snapshot_does_not_run_destructive_rollback(self) -> None:
        runner = mock.MagicMock(quiet=True)
        runner.run.side_effect = installer.OperationCancelled("cancelled")
        backup_path = pathlib.Path("/backups/example")
        version = installer.Version(1, 0, 0, 0)
        backup_manifest = installer.BackupManifest(
            version,
            installer.dt.datetime.now(installer.dt.timezone.utc),
            ("data",),
        )
        with (
            mock.patch.object(installer, "select_backup", return_value=backup_path),
            mock.patch.object(
                installer,
                "read_backup_manifest",
                return_value=backup_manifest,
            ),
            mock.patch.object(installer, "is_service_active", return_value=True),
            mock.patch.object(
                installer,
                "stage_backup_for_restore",
                return_value=(backup_path, pathlib.Path("/staging/source")),
            ),
            mock.patch.object(installer, "remove_staged_backup"),
            mock.patch.object(installer, "stop_service") as stop_service,
            mock.patch.object(installer, "start_service") as start_service,
        ):
            with self.assertRaises(installer.OperationCancelled):
                installer.restore_backup(
                    backup_path,
                    runner,
                    installed_version=version,
                    minimum_compatible_version=version,
                    has_install_state=True,
                )
        stop_service.assert_called_once_with(runner)
        start_service.assert_called_once_with(runner)
        self.assertEqual(runner.run.call_count, 1)

    def test_update_second_move_failure_restores_installation_directory(self) -> None:
        version = installer.Version(1, 0, 0, 0)
        next_version = installer.Version(1, 1, 0, 0)
        dotnet_path = pathlib.Path(sys.executable)
        manifest = installer.UpdateManifest(
            next_version, {}, "https://example.test/a.zip", "a" * 64, None
        )
        previous_state = installer.InstallState(
            version=version,
            minimum_compatible_version=None,
            dotnet_path=dotnet_path,
            installed_at=installer.dt.datetime.now(installer.dt.timezone.utc),
        )
        with tempfile.TemporaryDirectory() as temporary_directory:
            root = pathlib.Path(temporary_directory)
            install_root = root / "uzonmail"
            package_root = root / "package"
            temp_root = root / "temp"
            state_root = root / "state"
            unit_path = root / "uzon-mail.service"
            install_root.mkdir()
            package_root.mkdir()
            temp_root.mkdir()
            state_root.mkdir()
            (install_root / "old.txt").write_text("old", encoding="utf-8")
            (package_root / "new.txt").write_text("new", encoding="utf-8")
            unit_path.write_text("old unit", encoding="utf-8")
            state_path = state_root / "install-state.json"
            state_path.write_text("old state", encoding="utf-8")

            runner = mock.MagicMock(quiet=True)
            move_count = 0

            def run_command(arguments, **_kwargs):
                nonlocal move_count
                command = [os.fspath(value) for value in arguments]
                if command[0] == "mv":
                    move_count += 1
                    if move_count == 2:
                        raise installer.InstallerError("activate failed")
                    pathlib.Path(command[1]).rename(command[2])
                elif command[0] == "rm":
                    target = pathlib.Path(command[-1])
                    if target.is_dir():
                        import shutil

                        shutil.rmtree(target)
                    else:
                        target.unlink(missing_ok=True)
                elif command[0] == "install" and "-d" in command:
                    pathlib.Path(command[-1]).mkdir(parents=True, exist_ok=True)
                elif command[0] == "cp":
                    destination = pathlib.Path(command[-1])
                    destination.mkdir(parents=True, exist_ok=True)
                    for child in package_root.iterdir():
                        (destination / child.name).write_bytes(child.read_bytes())
                return mock.MagicMock(returncode=0)

            runner.run.side_effect = run_command
            with (
                mock.patch.object(installer, "INSTALL_ROOT", install_root),
                mock.patch.object(installer, "TEMP_ROOT", temp_root),
                mock.patch.object(installer, "INSTALLER_STATE_ROOT", state_root),
                mock.patch.object(installer, "INSTALL_STATE_PATH", state_path),
                mock.patch.object(installer, "SERVICE_UNIT_PATH", unit_path),
                mock.patch.object(installer, "get_installed_version", return_value=version),
                mock.patch.object(installer, "require_install_state", return_value=previous_state),
                mock.patch.object(installer, "validate_service_user"),
                mock.patch.object(installer, "fetch_update_manifest", return_value=manifest),
                mock.patch.object(installer, "ensure_dotnet_environment", return_value=dotnet_path),
                mock.patch.object(installer, "create_install_state", return_value=previous_state),
                mock.patch.object(installer, "prepare_package", return_value=package_root),
                mock.patch.object(installer, "set_installation_permissions"),
                mock.patch.object(installer, "is_service_active", return_value=True),
                mock.patch.object(installer, "is_service_enabled", return_value=True),
                mock.patch.object(installer, "stop_service"),
                mock.patch.object(installer, "start_service"),
                mock.patch.object(installer, "restore_system_file"),
                mock.patch.object(installer, "restore_service_state"),
            ):
                with self.assertRaisesRegex(installer.InstallerError, "activate failed"):
                    installer.update_application(runner)
            self.assertTrue((install_root / "old.txt").is_file())
            self.assertFalse(install_root.with_name("uzonmail.previous").exists())

    def test_restore_cleanup_failure_does_not_roll_back_committed_data(self) -> None:
        runner = mock.MagicMock(quiet=True)
        version = installer.Version(1, 0, 0, 0)
        backup_path = pathlib.Path("/backups/example")
        backup_manifest = installer.BackupManifest(
            version,
            installer.dt.datetime.now(installer.dt.timezone.utc),
            ("data",),
        )
        runner.run.side_effect = [
            mock.DEFAULT,
            installer.InstallerError("cleanup failed"),
        ]
        with (
            mock.patch.object(installer, "select_backup", return_value=backup_path),
            mock.patch.object(installer, "read_backup_manifest", return_value=backup_manifest),
            mock.patch.object(installer, "is_service_active", return_value=True),
            mock.patch.object(
                installer,
                "stage_backup_for_restore",
                return_value=(backup_path, pathlib.Path("/staging/source")),
            ),
            mock.patch.object(installer, "remove_staged_backup"),
            mock.patch.object(installer, "stop_service"),
            mock.patch.object(installer, "start_service"),
            mock.patch.object(installer, "copy_backup_contents") as copy_contents,
        ):
            restored = installer.restore_backup(
                backup_path,
                runner,
                installed_version=version,
                minimum_compatible_version=version,
                has_install_state=True,
            )
        self.assertEqual(restored, backup_path)
        copy_contents.assert_called_once_with(backup_path, backup_manifest, runner)
        self.assertEqual(runner.run.call_count, 2)


if __name__ == "__main__":
    unittest.main()
