#include "pch.h"
#include "Debug.h"


void Debug::Initialize()
{
	AllocConsole();

	FILE* dummy;

	freopen_s(&dummy, "CONOUT$", "w", stdout);
	freopen_s(&dummy, "CONOUT$", "w", stderr);
}

void Debug::Log(std::string message)
{
	
	std::cout << "[Debug]::" << message.c_str() << std::endl;
}

void Debug::Info(std::string message)
{

	std::cout << "[Info]::" << message.c_str() << std::endl;
}
