#include "RemoteHandle.h"
#include <exception>

RemoteHandle::RemoteHandle() : 
    _localHandle(INVALID_HANDLE_VALUE),
    _remoteHandle(INVALID_HANDLE_VALUE),
    _processId(0),
    _processHandle(INVALID_HANDLE_VALUE)
{
}


RemoteHandle::~RemoteHandle()
{
    Close();
    if (_processHandle != INVALID_HANDLE_VALUE)
    {
        CloseHandle(_processHandle);
        _processHandle = INVALID_HANDLE_VALUE;
    }
}

RemoteHandle& RemoteHandle::AssignHandle(HANDLE localHandle, DWORD processId)
{
    if (localHandle != _localHandle)
    {
        HANDLE remoteHandle = INVALID_HANDLE_VALUE;
        HANDLE processHandle = _processHandle;
        if (processId != _processId)
        {
            processHandle = OpenProcess(PROCESS_DUP_HANDLE, TRUE, processId);
            if (processHandle == nullptr)
            {
                processHandle = INVALID_HANDLE_VALUE;
            }
        }
        if ((processHandle != INVALID_HANDLE_VALUE) &&
            (!DuplicateHandle(GetCurrentProcess(), localHandle,
                processHandle, &remoteHandle, 0, TRUE, DUPLICATE_SAME_ACCESS)))
        {
            CloseHandle(processHandle);
            if (processId != _processId)
            {
                CloseHandle(_processHandle);
            }
            _processHandle = INVALID_HANDLE_VALUE;
            _processId = 0;
            remoteHandle = INVALID_HANDLE_VALUE;
        }
        Close();
        if (processId != _processId)
        {
            if (_processHandle != INVALID_HANDLE_VALUE)
            {
                CloseHandle(_processHandle);
            }
            _processHandle = processHandle;
            _processId = processId;
        }
        _localHandle = localHandle;
        _remoteHandle = remoteHandle;
    }
    return *this;
}

RemoteHandle& RemoteHandle::Close()
{
    if (_localHandle != INVALID_HANDLE_VALUE)
    {
        CloseHandle(_localHandle);
        _localHandle = INVALID_HANDLE_VALUE;
    }
    if (_remoteHandle != INVALID_HANDLE_VALUE)
    {
        DuplicateHandle(_processHandle, _remoteHandle, nullptr, nullptr, 0, TRUE, DUPLICATE_CLOSE_SOURCE);
        _remoteHandle = INVALID_HANDLE_VALUE;
    }
    return *this;
}

HANDLE RemoteHandle::GetLocalHandle() const
{
    return _localHandle;
}
HANDLE RemoteHandle::GetRemoteHandle() const
{
    return _remoteHandle;
}

RemoteHandle& RemoteHandle::DetachMove(RemoteHandle& destRemoteHandle)
{
    if (&destRemoteHandle == this)
    {
        return *this;
    }
    destRemoteHandle.Close();
    if (destRemoteHandle._processHandle != INVALID_HANDLE_VALUE)
    {
        CloseHandle(destRemoteHandle._processHandle);
    }
    destRemoteHandle._localHandle = _localHandle;
    destRemoteHandle._remoteHandle = _remoteHandle;
    destRemoteHandle._processId = _processId;
    destRemoteHandle._processHandle = _processHandle;
    _localHandle = INVALID_HANDLE_VALUE;
    _remoteHandle = INVALID_HANDLE_VALUE;
    _processId = 0;
    _processHandle = INVALID_HANDLE_VALUE;
    return *this;
}

bool RemoteHandle::IsValid() const
{
    return ((_localHandle != INVALID_HANDLE_VALUE) &&
        (_remoteHandle != INVALID_HANDLE_VALUE));
}

RemoteHandle& RemoteHandle::ResetRemoteProcessId(DWORD processId)
{
    if (processId == _processId)
    {
        return *this;
    }
    if (_remoteHandle != INVALID_HANDLE_VALUE)
    {
        DuplicateHandle(_processHandle, _remoteHandle, nullptr, nullptr, 0, TRUE, DUPLICATE_CLOSE_SOURCE);
        _remoteHandle = INVALID_HANDLE_VALUE;
    }
    if (_processHandle != INVALID_HANDLE_VALUE)
    {
        CloseHandle(_processHandle);
    }
    _processId = processId;
    _processHandle = OpenProcess(PROCESS_DUP_HANDLE, TRUE, processId);
    if ((_processHandle == nullptr) || (_processHandle == INVALID_HANDLE_VALUE))
    {
        _processHandle = INVALID_HANDLE_VALUE;
        _processId = 0;
        return *this;
    }
    if (!DuplicateHandle(GetCurrentProcess(), _localHandle,
        _processHandle, &_remoteHandle, 0, TRUE, DUPLICATE_SAME_ACCESS))
    {
        CloseHandle(_processHandle);
        _processHandle = INVALID_HANDLE_VALUE;
        _processId = 0;
        _remoteHandle = INVALID_HANDLE_VALUE;
    }
    return *this;
}
