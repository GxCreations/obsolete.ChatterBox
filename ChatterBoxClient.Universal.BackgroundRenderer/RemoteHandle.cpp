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
        HANDLE remoteHandle;
        HANDLE processHandle = _processHandle;
        if (processId != _processId)
        {
            processHandle = OpenProcess(PROCESS_DUP_HANDLE, TRUE, processId);
            if ((processHandle == nullptr) || (processHandle == INVALID_HANDLE_VALUE))
            {
                throw std::exception();
            }
        }
        if (!DuplicateHandle(GetCurrentProcess(), localHandle, processHandle, &remoteHandle, 0, TRUE, DUPLICATE_SAME_ACCESS))
        {
            throw std::exception();
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
