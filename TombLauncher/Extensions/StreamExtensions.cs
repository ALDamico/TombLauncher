﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TombLauncher.Extensions;

public static class StreamExtensions
{
    public static async Task CopyToAsync(this Stream source, Stream destination, int bufferSize, IProgress<long> progress = null, CancellationToken cancellationToken = default) {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        if (!source.CanRead)
            throw new ArgumentException("Has to be readable", nameof(source));
        if (destination == null)
            throw new ArgumentNullException(nameof(destination));
        if (!destination.CanWrite)
            throw new ArgumentException("Has to be writable", nameof(destination));
        if (bufferSize < 0)
            throw new ArgumentOutOfRangeException(nameof(bufferSize));

        var buffer = new byte[bufferSize];
        long totalBytesRead = 0;
        int bytesRead;
        var periodicTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(1000));
        if (progress != null)
            _ = RunInBackground(periodicTimer, () => progress?.Report(totalBytesRead), cancellationToken);
        while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) != 0) {
            await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
            totalBytesRead += bytesRead;
            //progress?.Report(totalBytesRead);
        }
        
        periodicTimer.Dispose();
    }
    
    private static async Task RunInBackground(PeriodicTimer periodicTimer, Action action, CancellationToken cancellationToken)
    {
        do
        {
            action();
        } while (await periodicTimer.WaitForNextTickAsync(cancellationToken));
    }
}