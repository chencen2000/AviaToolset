#include "pch.h"
#include "fdAppService.h"

using namespace System;


System::IO::Stream^ FDAppService::test() {
	String^ s = DateTime::Now.ToString("o");
	System::IO::MemoryStream^ ret = gcnew System::IO::MemoryStream(System::Text::UTF8Encoding::Default->GetBytes(s));
	return ret;
}

//bool FDAppService::start(int port) {
//	bool ret = false;
//	try {
//		String^ s = String::Format("http://localhost:{0}/", port);
//		_host = gcnew System::ServiceModel::ServiceHost(FDAppService::typeid, gcnew System::Uri(s));
//		System::ServiceModel::WebHttpBinding^ binding = gcnew System::ServiceModel::WebHttpBinding();
//		binding->HostNameComparisonMode = System::ServiceModel::HostNameComparisonMode::Exact;
//		System::ServiceModel::Description::ServiceEndpoint^ ep = _host->AddServiceEndpoint(IFDAppService::typeid, binding, s);
//		System::ServiceModel::Description::WebHttpBehavior^ httpBehavior = gcnew System::ServiceModel::Description::WebHttpBehavior();
//		ep->Behaviors->Add(httpBehavior);
//		_host->Open();
//		ret = true;
//	}
//	catch (System::Exception^) {
//
//	}
//	return ret;
//}

bool FDAppService::start(int port) {
	bool ret = false;
	try {
		System::String^ s = System::String::Format("http://localhost:{0}/", port);
		System::Uri^ uri = gcnew System::Uri(s);
		_host = gcnew System::ServiceModel::Web::WebServiceHost(FDAppService::typeid, uri);
		System::ServiceModel::WebHttpBinding^ binding = gcnew System::ServiceModel::WebHttpBinding();
		binding->HostNameComparisonMode = System::ServiceModel::HostNameComparisonMode::Exact;
		System::ServiceModel::Description::ServiceEndpoint^ ep = _host->AddServiceEndpoint(IFDAppService::typeid, binding, s);
		_host->Open();
		ret = true;
	}
	catch (System::Exception^) {}
	return ret;
}

void FDAppService::stop() {
	try {
		_host->Close();
	}
	catch (System::Exception^) {}
}
