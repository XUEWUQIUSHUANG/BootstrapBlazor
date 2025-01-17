﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License
// See the LICENSE file in the project root for more information.
// Maintainer: Argo Zhang(argo@live.ca) Website: https://www.blazor.zone

namespace BootstrapBlazor.Components;

/// <summary>
/// 串口通讯类
/// </summary>
class SerialPort(JSModule jsModule, string serialPortId) : ISerialPort
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public bool IsOpen { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public Func<byte[], Task>? DataReceive { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    public async Task<bool> Write(byte[] data, CancellationToken token = default)
    {
        var ret = false;
        if (IsOpen)
        {
            ret = await jsModule.InvokeAsync<bool>("write", token, serialPortId, data);
        }
        return ret;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    public async Task<bool> Open(SerialPortOptions options, CancellationToken token = default)
    {
        DotNetObjectReference<SerialPort>? interop = null;
        if (DataReceive != null)
        {
            interop = DotNetObjectReference.Create(this);
        }
        IsOpen = await jsModule.InvokeAsync<bool>("open", token, serialPortId, interop, nameof(DataReceiveCallback), options);
        return IsOpen;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    public async Task<bool> Close(CancellationToken token = default)
    {
        var ret = await jsModule.InvokeAsync<bool>("close", token, serialPortId);
        if (ret)
        {
            IsOpen = false;
        }
        return ret;
    }

    /// <summary>
    /// 接收数据回调方法 由 Javascript 调用
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [JSInvokable]
    public async Task DataReceiveCallback(byte[] data)
    {
        if (DataReceive != null)
        {
            await DataReceive(data);
        }
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<SerialPortUsbInfo?> GetUsbInfo(CancellationToken token = default) => await jsModule.InvokeAsync<SerialPortUsbInfo>("getInfo", token, serialPortId);

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<SerialPortSignals?> GetSignals(CancellationToken token = default) => await jsModule.InvokeAsync<SerialPortSignals>("getSignals", token, serialPortId);

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="options"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<bool> SetSignals(SerialPortSignalsOptions options, CancellationToken token = default) => await jsModule.InvokeAsync<bool>("setSignals", token, serialPortId, options);

    private async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            await jsModule.InvokeVoidAsync("dispose", serialPortId);
        }
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsync(true);
        GC.SuppressFinalize(this);
    }
}
