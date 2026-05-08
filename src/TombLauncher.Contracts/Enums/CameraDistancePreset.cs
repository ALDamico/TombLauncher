namespace TombLauncher.Contracts.Enums;

file class CameraDistanceConstants
{
    internal const int BlockSquareSideLength = 1024;
}

public enum CameraDistancePreset
{
    Custom = 0,
    One = CameraDistanceConstants.BlockSquareSideLength,
    OneAndAHalf = (int)(CameraDistanceConstants.BlockSquareSideLength * 1.5),
    Two = CameraDistanceConstants.BlockSquareSideLength * 2,
    TwoAndAHalf = (int)(CameraDistanceConstants.BlockSquareSideLength * 2.5),
    Three = CameraDistanceConstants.BlockSquareSideLength * 3
}