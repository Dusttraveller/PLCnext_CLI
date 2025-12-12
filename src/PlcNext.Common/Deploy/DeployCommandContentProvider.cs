#region Copyright
///////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) Phoenix Contact GmbH & Co KG
//  This software is licensed under Apache-2.0
//
///////////////////////////////////////////////////////////////////////////////
#endregion

using System;
using System.IO;
using System.Linq;
using PlcNext.Common.Build;
using PlcNext.Common.Commands;
using PlcNext.Common.DataModel;
using PlcNext.Common.Project;
using PlcNext.Common.Tools;
using PlcNext.Common.Tools.FileSystem;
using PlcNext.Common.Tools.IO;
using PlcNext.Common.Tools.Priority;
using PlcNext.Common.Tools.UI;

namespace PlcNext.Common.Deploy
{
    internal class DeployCommandContentProvider : PriorityContentProvider
    {
        public override SubjectIdentifier LowerPrioritySubject { get; } = new SubjectIdentifier(nameof(CommandDefinitionContentProvider));
        private readonly IFileSystem fileSystem;
        private readonly IPasswordProvider passwordProvider;
        private readonly IUserInterface userInterface;

        public DeployCommandContentProvider(IFileSystem fileSystem, IPasswordProvider passwordProvider, IUserInterface userInterface)
        {
            this.fileSystem = fileSystem;
            this.passwordProvider = passwordProvider;
            this.userInterface = userInterface;
        }

        public override bool CanResolve(Entity owner, string key, bool fallback = false)
        {
            CommandEntity command = CommandEntity.Decorate(owner);
            return (key == Constants.OutputArgumentName &&
                    command.CommandName.Equals("deploy", StringComparison.OrdinalIgnoreCase)) ||
                   (key == EntityKeys.InternalDeployPathKey &&
                    TargetEntity.Decorate(owner).HasFullName)||
                   key == EntityKeys.InternalConfigPathKey ||
                   key == EntityKeys.InternalSigningKey ||
                   key == EntityKeys.InternalManipulateVersionKey;
        }

        public override Entity Resolve(Entity owner, string key, bool fallback = false)
        {
            switch (key)
            {
                case EntityKeys.InternalDeployPathKey:
                    VirtualDirectory deployRoot = GetDeployRoot();
                    return owner.Create(key, deployRoot.FullName, deployRoot);
         
                case EntityKeys.InternalConfigPathKey:
                    VirtualDirectory configDirectory = GetConfigDirectory();
                    return owner.Create(key, configDirectory.FullName, configDirectory);

                case EntityKeys.InternalSigningKey:
                    bool signing = GetSigningOption();
                    return owner.Create(key, signing.ToString(), signing);
                
                case EntityKeys.InternalManipulateVersionKey:
                    string manipulatedVersion = GetManipulatedVersion();
                    return owner.Create(key, manipulatedVersion);

                case Constants.OutputArgumentName:
                default:
                    string outputDirectory = GetOutput();
                    return owner.Create(key, outputDirectory);
            }

            string GetOutput()
            {
                CommandEntity command = CommandEntity.Decorate(owner);
                FileEntity projectFileEntity = FileEntity.Decorate(owner.Root);
                string outputDirectory = command.GetSingleValueArgument(Constants.OutputArgumentName);

                if (string.IsNullOrEmpty(outputDirectory))
                {
                    outputDirectory = projectFileEntity.Directory
                                                       .Directory(Constants.LibraryFolderName)
                                                       .FullName;
                }
                return outputDirectory;
            }

            VirtualDirectory GetDeployRoot()
            {
                TargetEntity targetEntity = TargetEntity.Decorate(owner);
                BuildEntity buildEntity = BuildEntity.Decorate(owner);
                Entity project = owner.Root;
                CommandEntity commandOrigin = CommandEntity.Decorate(owner.Origin);
                VirtualDirectory outputRoot = fileSystem.GetDirectory(commandOrigin.Output, project.Path);
                VirtualDirectory deployRoot = outputRoot.Directory(targetEntity.FullName.Replace(',', '_'),
                                                                   buildEntity.BuildType);
                return deployRoot;
            }       
            VirtualDirectory GetConfigDirectory()
            {
                Entity project = owner.Root;
                CommandEntity commandOrigin = CommandEntity.Decorate(owner.Origin);
                VirtualDirectory configDirectory = fileSystem.GetDirectory(commandOrigin.Output, project.Path).Directory("config");
                return configDirectory;
            }  
            
            string GetManipulatedVersion()
            {
                Entity project = owner.Root;
                CommandEntity command = CommandEntity.Decorate(owner.Origin);
                if (command.IsCommandArgumentSpecified(Constants.ManipulatedVersionArgumentKey))
                {
                    return command.GetSingleValueArgument(Constants.ManipulatedVersionArgumentKey);
                }
                return string.Empty;
            }

            bool GetSigningOption()
            {
                CommandEntity command = CommandEntity.Decorate(owner.Origin);
                ProjectConfigurations config = ProjectEntity.Decorate(owner.Root).Configuration;
                bool sign = command.GetBoolValueArgument(Constants.SignArgumentKey);

                if (sign || (config.IsPersistent && config.SignRequested))
                {
                    SetCertificatesAndKeyFilesOptions();

                    SetTimestampOptions();

                    SetPasswordOptions();

                    SetTimestampConfigurationOptions();

                    return true;
                }
                else
                {
                    if (command.IsCommandArgumentSpecified(Constants.pkcs12ArgumentKey) ||
                        command.IsCommandArgumentSpecified(Constants.privateKeyArgumentKey) ||
                        command.IsCommandArgumentSpecified(Constants.signingCertificateArgumentKey) ||
                        command.IsCommandArgumentSpecified(Constants.certificateChainArgumentKey) ||
                        command.GetBoolValueArgument(Constants.timestampArgumentKey) == true ||
                        command.GetBoolValueArgument(Constants.noTimestampArgumentKey) == true ||
                        command.IsCommandArgumentSpecified(Constants.passwordArgumentKey) ||
                        command.IsCommandArgumentSpecified(Constants.timestampConfiguration))
                    {
                        throw new SignOptionMissingException();
                    }

                    if (!string.IsNullOrEmpty(config.PKCS12) ||
                        !string.IsNullOrEmpty(config.PrivateKey) ||
                        !string.IsNullOrEmpty(config.SigningCertificate) ||
                        (config.CertificateChain != null && config.CertificateChain.Any()) ||
                        config.Timestamp == true ||
                        config.NoTimestamp == true ||
                        !string.IsNullOrEmpty(config.TimestampConfiguration))
                    {
                        userInterface.WriteInformation("Signing was not requested but at least one of the following arguments is specified in the configuration: " +
                            $"{Constants.pkcs12ArgumentKey}, {Constants.privateKeyArgumentKey}, {Constants.signingCertificateArgumentKey}" +
                            $", {Constants.certificateChainArgumentKey}, {Constants.timestampArgumentKey}, {Constants.noTimestampArgumentKey}" +
                            $", {Constants.timestampConfiguration}");
                    }

                    return false;
                }


                void SetCertificatesAndKeyFilesOptions()
                {
                    if (command.IsCommandArgumentSpecified(Constants.pkcs12ArgumentKey))
                    {
                        if (command.IsCommandArgumentSpecified(Constants.privateKeyArgumentKey) ||
                            command.IsCommandArgumentSpecified(Constants.signingCertificateArgumentKey) ||
                            command.IsCommandArgumentSpecified(Constants.certificateChainArgumentKey) ||
                            (config.IsPersistent && (!string.IsNullOrEmpty(config.PrivateKey) ||
                                                     !string.IsNullOrEmpty(config.SigningCertificate) ||
                                                     (config.CertificateChain != null && config.CertificateChain.Any()))
                            ))
                        {
                            throw new SignOptionWrongCombinationException("PKCS#12", "PEM");
                        }

                        if (config.IsPersistent && !string.IsNullOrEmpty(config.PKCS12))
                        {
                            userInterface.WriteInformation("PKCS#12 entry in configuration file will be ignored.");
                        }
                        owner.AddEntity(owner.Create(Constants.pkcs12ArgumentKey, command.GetSingleValueArgument(Constants.pkcs12ArgumentKey)), EntityKeys.InternalPKCS12Key);
                    }
                    else
                    {
                        if (config.IsPersistent && !string.IsNullOrEmpty(config.PKCS12))
                        {
                            if (command.IsCommandArgumentSpecified(Constants.privateKeyArgumentKey) ||
                            command.IsCommandArgumentSpecified(Constants.signingCertificateArgumentKey) ||
                            command.IsCommandArgumentSpecified(Constants.certificateChainArgumentKey) ||
                            (config.IsPersistent && (!string.IsNullOrEmpty(config.PrivateKey) ||
                                                     !string.IsNullOrEmpty(config.SigningCertificate) ||
                                                     (config.CertificateChain != null && config.CertificateChain.Any()))
                            ))
                            {
                                throw new SignOptionWrongCombinationException("PKCS#12", "PEM files");
                            }

                            owner.AddEntity(owner.Create(Constants.pkcs12ArgumentKey, config.PKCS12), EntityKeys.InternalPKCS12Key);
                        }
                        else
                        {
                            //create entity without value
                            owner.AddEntity(owner.Create(Constants.pkcs12ArgumentKey), EntityKeys.InternalPKCS12Key);

                            //pkcs12 is not set, privatekey and signingcertificate must be set

                            if (command.IsCommandArgumentSpecified(Constants.privateKeyArgumentKey))
                            {
                                owner.AddEntity(owner.Create(Constants.privateKeyArgumentKey, command.GetSingleValueArgument(Constants.privateKeyArgumentKey)), EntityKeys.InternalPrivateKeyKey);

                            }
                            else
                            {
                                if (config.IsPersistent && !string.IsNullOrEmpty(config.PrivateKey))
                                {
                                    owner.AddEntity(owner.Create(Constants.privateKeyArgumentKey, config.PrivateKey), EntityKeys.InternalPrivateKeyKey);
                                }
                                else
                                {
                                    throw new SignOptionMissingKeyFilesException();
                                }
                            }

                            if (command.IsCommandArgumentSpecified(Constants.signingCertificateArgumentKey))
                            {
                                owner.AddEntity(owner.Create(Constants.signingCertificateArgumentKey, command.GetSingleValueArgument(Constants.signingCertificateArgumentKey)), EntityKeys.InternalSigningCertKey);
                            }
                            else
                            {
                                if (config.IsPersistent && !string.IsNullOrEmpty(config.SigningCertificate))
                                {
                                    owner.AddEntity(owner.Create(Constants.signingCertificateArgumentKey, config.SigningCertificate), EntityKeys.InternalSigningCertKey);
                                }
                                else
                                {
                                    throw new SignOptionMissingKeyFilesException();
                                }
                            }


                            if (command.IsCommandArgumentSpecified(Constants.certificateChainArgumentKey))
                            {
                                owner.AddEntity(owner.Create(Constants.certificateChainArgumentKey, command.GetMultiValueArgument(Constants.certificateChainArgumentKey)), EntityKeys.InternalCertificateChainKey);
                            }
                            else
                            {
                                if (config.IsPersistent && config.CertificateChain != null && config.CertificateChain.Any())
                                {
                                    owner.AddEntity(owner.Create(Constants.certificateChainArgumentKey, config.CertificateChain), EntityKeys.InternalCertificateChainKey);
                                }
                                else
                                {
                                    // create empty entity because optional argument was not specified
                                    owner.AddEntity(owner.Create(Constants.certificateChainArgumentKey), EntityKeys.InternalCertificateChainKey);
                                }
                            }
                        }
                    }
                }

                void SetTimestampOptions()
                {
                    if (command.GetBoolValueArgument(Constants.timestampArgumentKey) == false &&
                        command.GetBoolValueArgument(Constants.noTimestampArgumentKey) == false &&
                        config.Timestamp == false &&
                        config.NoTimestamp == false)
                    {
                        throw new SignOptionMissingTimestampDecisionException();
                    }


                    if (command.GetBoolValueArgument(Constants.timestampArgumentKey) == true)
                    {
                        if (command.GetBoolValueArgument(Constants.noTimestampArgumentKey) == true
                            || config.NoTimestamp == true)
                        {
                            throw new SignOptionWrongCombinationException("timestamp", "notimestamp");
                        }
                        
                        owner.AddEntity(owner.Create(Constants.timestampArgumentKey, true), EntityKeys.InternalTimestampKey);
                    }
                    else 
                    {
                        if (config.IsPersistent && config.Timestamp == true)
                        {
                            if (command.GetBoolValueArgument(Constants.noTimestampArgumentKey) == true
                                || config.NoTimestamp == true)
                            {
                                throw new SignOptionWrongCombinationException("timestamp", "notimestamp");
                            }
                            owner.AddEntity(owner.Create(Constants.timestampArgumentKey, true), EntityKeys.InternalTimestampKey);
                        }
                    }

                    if (command.GetBoolValueArgument(Constants.noTimestampArgumentKey) == true)
                    {
                        owner.AddEntity(owner.Create(Constants.timestampArgumentKey, false), EntityKeys.InternalTimestampKey);
                    }
                    else
                    {
                        if (config.IsPersistent && config.NoTimestamp == true)
                        {
                            owner.AddEntity(owner.Create(Constants.timestampArgumentKey, false), EntityKeys.InternalTimestampKey);
                        }
                    }
                }

                void SetPasswordOptions()
                {
                    string password = string.Empty;
                    if (!command.IsCommandArgumentSpecified(Constants.passwordArgumentKey))
                    {
                        try
                        {
                            password = passwordProvider.ProvidePassword();
                        }
                        catch (InvalidOperationException) 
                        {
                            password = string.Empty;
                        }
                        catch (IOException)
                        {
                            password = string.Empty;
                        }
                    }
                    else
                    {
                        password = command.GetSingleValueArgument(Constants.passwordArgumentKey);
                    }

                    if (string.IsNullOrEmpty(password))
                    {
                        throw new SignOptionMissingPasswordException();
                    }
                    owner.AddEntity(owner.Create(Constants.passwordArgumentKey, password), EntityKeys.InternalPasswordKey);
                }

                void SetTimestampConfigurationOptions()
                {
                    owner.AddEntity(owner.Create(Constants.timestampConfiguration, command.IsCommandArgumentSpecified(Constants.timestampConfiguration)
                                                                                    ? command.GetSingleValueArgument(Constants.timestampConfiguration)
                                                                                    : (config.IsPersistent && !string.IsNullOrEmpty(config.TimestampConfiguration)
                                                                                        ? config.TimestampConfiguration
                                                                                        : null)), EntityKeys.InternalTimestampConfigKey);
                }
            }
        }
    }
}
