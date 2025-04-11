# Mastersign WPF Code Editor

> WPF control for editing code with syntax highlighting and intellisense

## Intention

This control is intended to be used as a powerful editor for
data structures in a WPF application.
The main goal is to provide a YAML and JSON editor with JSON schema support.
With this capability it is well suited for editing configuration files and data models.

It is supposed to support editing individual files without further context.

It is designed to run on modern Windows 11 systems without additional setup.

## Background

The control makes use of the Monaco editor from Visual Studio Code,
with the `monaco-yaml` plug-in.
The Monaco editor is rendered in WebView2.
Which is an embedded instance of a specially configured version of Microsofts Edge browser.

This stack is optimized for the reuse of resources.
Because WebView2 makes use of the Edge browser installed with Windows.
And even if multiple instances of the Mastersign WPF Code Editor
are displayed, in the background only one instance of the Edge browser engine
is running and therefore consumes memory.

## Limitations

The control does not well support editing code for programming.
Because it does not load a project context or a language server.
It is focused on editing structurally formatted text files.

## Requirements

The control targets .NET Framework 4.8, which is installed by default
on Windows 10 and Windows 11.

The control makes use of WebView2 and expects the WebView2 runtime to be installed on the system.
For current Windows 11 systems, and even the most Windows 10 systems, this is given.
