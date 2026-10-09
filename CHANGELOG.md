# Changelog

All notable changes to this package are documented here.
The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [Unreleased]

### Added

- Setup step 「VRChat の設定」: ignores VCC-managed package folders listed in
  `Packages/vpm-manifest.json` through a `shiori-vrchat` block in `.gitignore`; offers an update
  when the package set changes; counts as done in projects that are not managed by VCC.
- Build target (PC / Android / iOS) shown at the top of simple mode.
- Re-import warning in the 戻す confirmation and a creator-oriented hint under 保存.
- Package skeleton: `Shiori.VRChat.Editor` (no VRChat SDK reference), tests with an in-memory
  `IExtensionContext`, and `Tools~/Add-ToDevProject.ps1` to join the core's dev projects.
