#include "pch.h"
#include "fdAppService.h"

using namespace System;

int main(array<System::String ^> ^args)
{
	FDAppService^ service = gcnew FDAppService();
	if (service->start(12345)) {
		System::Console::WriteLine("Press any key to terminate.");
		System::Console::ReadKey();

		service->stop();
	}
	return 0;
}
