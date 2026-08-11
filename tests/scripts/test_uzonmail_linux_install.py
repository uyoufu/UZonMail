"""Tests for the standalone UzonMail Linux installer."""

from __future__ import annotations

import importlib.util
import json
import os
import pathlib
import stat
import sys
import tempfile
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


class ManifestTests(unittest.TestCase):
    """Verify update metadata validation without reading dependency entries."""

    def test_infer_linux_package_url_preserves_parent_and_query(self) -> None:
        result = installer.infer_linux_package_url(
            "https://example.test/releases/uzonmail-desktop-win-x64-0.23.5.0.zip?token=abc",
            installer.Version.parse("0.23.5.0"),
        )
        self.assertEqual(
            result,
            "https://example.test/releases/uzonmail-service-linux-x64-0.23.5.0.zip?token=abc",
        )

    def test_infer_linux_package_url_requires_https(self) -> None:
        with self.assertRaises(installer.InstallerError):
            installer.infer_linux_package_url(
                "http://example.test/package.zip", installer.Version.parse("1.0.0.0")
            )

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
        self.assertIn("uzonmail-service-linux-x64-0.23.5.0.zip", manifest.package_url)

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


class ConfigurationTests(unittest.TestCase):
    """Verify secure cached input and production override generation."""

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
                mock.patch.object(installer, "TEMP_CONFIG_PATH", config_path),
            ):
                installer.save_cached_install_config(config)
                loaded = installer.load_cached_install_config()
            self.assertEqual(loaded, config)
            if os.name != "nt":
                self.assertEqual(config_path.stat().st_mode & 0o777, 0o600)


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
                    "contents": ["data"],
                }
            ),
            encoding="utf-8",
        )
        (backup / "data").mkdir()
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
            with self.assertRaisesRegex(installer.InstallerError, "symbolic link"):
                installer.validate_backup_tree(backup)


class ServiceAndAssemblyTests(unittest.TestCase):
    """Verify the generated unit and managed assembly version reader."""

    def test_service_unit_uses_dedicated_account_and_production_environment(self) -> None:
        unit = installer.service_unit_content()
        self.assertIn("User=uzonmail", unit)
        self.assertIn("Group=uzonmail", unit)
        self.assertIn("ASPNETCORE_ENVIRONMENT=Production", unit)
        self.assertIn("/usr/bin/dotnet /var/www/uzonmail/UzonMailService.dll", unit)

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
        runner.run.side_effect = installer.OperationCancelled("cancelled")
        with tempfile.TemporaryDirectory() as temporary_directory:
            with (
                mock.patch.object(
                    installer, "get_installed_version", return_value=installer.Version(1, 0, 0, 0)
                ),
                mock.patch.object(installer, "is_service_active", return_value=True),
                mock.patch.object(installer, "stop_service") as stop_service,
                mock.patch.object(installer, "start_service") as start_service,
                mock.patch.object(installer, "invoking_user", return_value=("owner", "owner")),
            ):
                with self.assertRaises(installer.OperationCancelled):
                    installer.create_backup(
                        pathlib.Path(temporary_directory), runner, restart_service=False
                    )
        stop_service.assert_called_once_with(runner)
        start_service.assert_called_once_with(runner, confirm=False)

    def test_restore_cancelled_before_snapshot_does_not_run_destructive_rollback(self) -> None:
        runner = mock.MagicMock(quiet=True)
        runner.run.side_effect = installer.OperationCancelled("cancelled")
        backup_path = pathlib.Path("/backups/example")
        version = installer.Version(1, 0, 0, 0)
        with (
            mock.patch.object(installer, "read_install_state", return_value=None),
            mock.patch.object(installer, "select_backup", return_value=backup_path),
            mock.patch.object(
                installer,
                "read_backup_manifest",
                return_value=(version, installer.dt.datetime.now(installer.dt.timezone.utc)),
            ),
            mock.patch.object(installer, "is_service_active", return_value=True),
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
        start_service.assert_called_once_with(runner, confirm=False)
        self.assertEqual(runner.run.call_count, 1)


if __name__ == "__main__":
    unittest.main()
