# Test Plan

## Automated Unit Tests

- Technical tunnel names are generated correctly.
- Special characters and umlauts in display names are sanitized.
- IDs contain eight hexadecimal characters.
- Only services with `WireGuardTunnel$wgUserControl_` are recognized as managed.
- Foreign WireGuard services are ignored.
- Metadata can be saved and loaded.
- Missing metadata returns an empty list.
- DACL merging extends an existing `S-1-5-4` allow ACE without creating duplicates.
- Log sanitizing removes PrivateKey lines.

## Manual or Integration Tests on a Windows Test System

- Start a stopped WireGuard tunnel service as a standard user.
- Stop a running WireGuard tunnel service as a standard user.
- Missing service permissions produce a clear error message.
- Standard users cannot read the configuration file.
- Import rollback removes copied files and installed services after failures.
- Uninstallation removes only managed tunnels.
- Logs do not contain keys or full configuration contents.
