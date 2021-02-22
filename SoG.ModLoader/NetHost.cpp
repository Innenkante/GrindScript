#include "pch.h"
#include "Debug.h"
#include "NetHost.h"




void NetHost::StartCLR()
{
	ICLRMetaHost* metaHost = nullptr;
	
	CLRCreateInstance(CLSID_CLRMetaHost, IID_ICLRMetaHost, reinterpret_cast<LPVOID*>(&metaHost));

	ICLRRuntimeInfo* runtimeInfo = NULL;
	metaHost->GetRuntime(L"v4.0.30319", IID_ICLRRuntimeInfo, (LPVOID*)&runtimeInfo);

	ICLRRuntimeHost* clrRuntimeHost = NULL;
	runtimeInfo->GetInterface(CLSID_CLRRuntimeHost, IID_ICLRRuntimeHost, (LPVOID*)&clrRuntimeHost);

	DWORD result = 0;
	clrRuntimeHost->ExecuteInDefaultAppDomain(L"GrindScript.dll", L"SoG.GrindScript.NativeInterface", L"Initialize", L"Test", &result);

	Debug::Info("Started NativeInterface::Initialize");
}

//void NetHost::LoadMods(std::string path)
//{
//}

