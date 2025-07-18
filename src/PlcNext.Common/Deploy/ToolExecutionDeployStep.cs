#region Copyright
///////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) Phoenix Contact GmbH & Co KG
//  This software is licensed under Apache-2.0
//
///////////////////////////////////////////////////////////////////////////////
#endregion

using Autofac.Features.OwnedInstances;
using PlcNext.Common.Commands;
using PlcNext.Common.DataModel;
using PlcNext.Common.Templates;
using PlcNext.Common.Templates.Description;
using PlcNext.Common.Tools;
using PlcNext.Common.Tools.DynamicCommands;
using PlcNext.Common.Tools.Events;
using PlcNext.Common.Tools.Process;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlcNext.Common.Deploy
{
    internal class ToolExecutionDeployStep : IDeployStep
    {
        public string Identifier => "ToolExecutionDeployStep";
        public readonly string toolnameArgumentKey = "toolname";
        public readonly string tooloptionsArgumentKey = "tooloptions";

        private readonly ITemplateResolver templateResolver;
        private readonly IProcessManager processManager;
        private readonly ExecutionContext executionContext;
        private readonly IBinariesLocator binariesLocator;
        private readonly ITemplateRepository templateRepository;

        public ToolExecutionDeployStep(ITemplateResolver templateResolver, 
                                       IProcessManager processManager, 
                                       ExecutionContext executionContext,
                                       IBinariesLocator binariesLocator,
                                       ITemplateRepository templateRepository)
        {
            this.templateResolver = templateResolver;
            this.processManager = processManager;
            this.executionContext = executionContext;
            this.binariesLocator = binariesLocator;
            this.templateRepository = templateRepository;
        }

        public void Execute(Entity dataModel, ChangeObservable observable, templateDeployPostStep currentDeployStep)
        {
            if (currentDeployStep.Arguments?.Length != 1)
            {
                throw new ArgumentException("Each ToolExecutionDeployStep defined in the template has to contain exactly one argument with a unique identifier as name.");
            }
            string deployStepName = currentDeployStep.Arguments.First().name;
                        
            TemplateEntity template = TemplateEntity.Decorate(dataModel.Root);

            string options = FindToolOptions(); 
            string tool = FindExecutable();
            ExecuteTool(tool, options);


            void ExecuteTool(string tool, string arguments)
            {
                using (IProcess process = processManager.StartProcess(tool, arguments, executionContext))
                {
                    process.WaitForExit();
                    if (process.ExitCode != 0)
                    {
                        throw new FormattableException($"Deploy step {deployStepName} failed!");
                    }
                }
            }

            string FindExecutable()
            {
                string toolname = currentDeployStep.FixedArguments?.Where(argument => argument.name == toolnameArgumentKey)?.Select(argument => argument.value)?.FirstOrDefault();
                if (toolname == null)
                {
                    throw new ArgumentException("Each ToolExecutionDeployStep defined in the template has to contain exactly one fixed argument with name 'toolname'");
                }

                //try find executable in file-names.xml in template location
                string templateBase = templateRepository.GetTemplateBase(template.Template);

                string result = binariesLocator.GetExecutableCommand(toolname, templateBase);

                if (result != null)
                {
                    return result;
                }

                //try find executable in file-names.xml in plcncli directory
                result = binariesLocator.GetExecutableCommand(toolname);

                if (result == null)
                {
                    throw new FormattableException($"Tool {toolname} not found. Execution not possible.");
                }

                return result;
            }

            string FindToolOptions()
            {
                string tooloptions = currentDeployStep.FixedArguments?.Where(argument => argument.name == tooloptionsArgumentKey)?.Select(argument => argument.value)?.FirstOrDefault();
                if (tooloptions == null)
                {
                    throw new ArgumentException("Each ToolExecutionDeployStep defined in the template has to contain exactly one fixed argument with name 'tooloptions'");
                }
                string result = templateResolver.Resolve(tooloptions, template)?.TrimEnd();
                
                
                CommandEntity command = CommandEntity.FindUpperCommand(dataModel);
                if (command.IsCommandArgumentSpecified(deployStepName))
                {
                    result += " " + command.GetSingleValueArgument(deployStepName)?.Trim();
                }
                return result;
            }

        }
    }
}
