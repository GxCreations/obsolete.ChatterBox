#pragma once
#define WIN32_LEAN_AND_MEAN
#include <Windows.h>

class RemoteHandle
{
public:
    RemoteHandle();
    ~RemoteHandle();
    RemoteHandle& AssignHandle(HANDLE localHandle, DWORD processId);
    RemoteHandle& Close();
    HANDLE GetLocalHandle() const;
    HANDLE GetRemoteHandle() const;
private:
    RemoteHandle(const RemoteHandle&);
    const RemoteHandle& operator = (const RemoteHandle&) { return *this;  };
    HANDLE _localHandle;
    HANDLE _remoteHandle;
    DWORD _processId;
    HANDLE _processHandle;
};

