#pragma once

[System::ServiceModel::ServiceContract]
interface class IFDAppService
{
	[System::ServiceModel::OperationContract]
	[System::ServiceModel::Web::WebGet]
	System::IO::Stream^ test();
};

ref class FDAppService : IFDAppService
{
public:
	virtual System::IO::Stream^ test();

	bool start(int port);
	void stop();

private:
	System::ServiceModel::ServiceHost^ _host;


};