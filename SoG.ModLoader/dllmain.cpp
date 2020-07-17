// dllmain.cpp : Definiert den Einstiegspunkt für die DLL-Anwendung.
#include "pch.h"
#include "Debug.h"
#include "NetHost.h"

DWORD WINAPI MainThread(LPVOID arg)
{
    NetHost::StartCLR();
    return 1;
}
BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        Debug::Initialize();
        Debug::Info("Loaded modloader.dll");
        Debug::Info("Started the .net enviorment");
    	
        CreateThread(nullptr, 0, &MainThread, nullptr, 0, nullptr);
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

