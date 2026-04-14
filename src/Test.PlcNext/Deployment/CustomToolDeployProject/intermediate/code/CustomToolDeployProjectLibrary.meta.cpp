#include "Arp/System/Core/Arp.h"
#include "Arp/Plc/Commons/Meta/TypeSystem/TypeSystem.h"
#include "CustomToolDeployProjectLibrary.hpp"

namespace CustomToolDeployProject
{

using namespace Arp::Plc::Commons::Meta;
    
    void CustomToolDeployProjectLibrary::InitializeTypeDomain()
    {
        this->typeDomain.AddTypeDefinitions
        (
            // Begin TypeDefinitions
            {
            }
            // End TypeDefinitions
        );
    }

} // end of namespace CustomToolDeployProject

