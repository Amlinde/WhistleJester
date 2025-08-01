{ pkgs ? import <nixpkgs> {} }:
  pkgs.mkShell {
    nativeBuildInputs = with pkgs.buildPackages; [ p7zip dotnetCorePackages.dotnet_9.sdk ];
}
