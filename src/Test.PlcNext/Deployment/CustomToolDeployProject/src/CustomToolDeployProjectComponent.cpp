#include "CustomToolDeployProjectComponent.hpp"
#include "Arp/Plc/Commons/Domain/PlcDomainProxy.hpp"
#include "CustomToolDeployProjectLibrary.hpp"

namespace CustomToolDeployProject
{

using namespace Arp::Plc::Commons::Domain;
#if ARP_ABI_VERSION_MAJOR < 2
CustomToolDeployProjectComponent::CustomToolDeployProjectComponent(IApplication& application, const String& name)
: ComponentBase(application, ::CustomToolDeployProject::CustomToolDeployProjectLibrary::GetInstance(), name, ComponentCategory::Custom)
, MetaComponentBase(::CustomToolDeployProject::CustomToolDeployProjectLibrary::GetInstance().GetNamespace())
#else
CustomToolDeployProjectComponent::CustomToolDeployProjectComponent(ILibrary& library, const String& name)
    : ComponentBase(library, name, ComponentCategory::Custom, GetDefaultStartOrder())
    , MetaComponentBase(::CustomToolDeployProject::CustomToolDeployProjectLibrary::GetInstance().GetNamespace())
#endif
{
}

void CustomToolDeployProjectComponent::Initialize()
{
    // never remove next line
    PlcDomainProxy::GetInstance().RegisterComponent(*this, true);
    
    // subscribe events from the event system (Nm) here
}

void CustomToolDeployProjectComponent::SubscribeServices()
{
	// subscribe the services used by this component here
}

void CustomToolDeployProjectComponent::LoadSettings(const String& /*settingsPath*/)
{
	// load firmware settings here
}

void CustomToolDeployProjectComponent::SetupSettings()
{
    // never remove next line
    MetaComponentBase::SetupSettings();

	// setup firmware settings here
}

void CustomToolDeployProjectComponent::PublishServices()
{
	// publish the services of this component here
}

void CustomToolDeployProjectComponent::LoadConfig()
{
    // load project config here
}

void CustomToolDeployProjectComponent::SetupConfig()
{
    // setup project config here
}

void CustomToolDeployProjectComponent::ResetConfig()
{
    // implement this inverse to SetupConfig() and LoadConfig()
}

void CustomToolDeployProjectComponent::Dispose()
{
    // never remove next line
    MetaComponentBase::Dispose();

	// implement this inverse to SetupSettings(), LoadSettings() and Initialize()
}

void CustomToolDeployProjectComponent::PowerDown()
{
	// implement this only if data shall be retained even on power down event
	// will work only for PLCnext controllers with an "Integrated uninterruptible power supply (UPS)"
	// Available with 2021.6 FW
}

} // end of namespace CustomToolDeployProject
