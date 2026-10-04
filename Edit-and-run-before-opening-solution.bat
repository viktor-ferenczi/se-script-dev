@echo off

REM Location of the local Torch instance (Torch.Server.exe and DedicatedServer64 folder),
REM referenced by the TorchPlugin and DedicatedPlugin projects
mklink /J Torch "C:\Torch"

pause
