﻿using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.Operations;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;

namespace Vsts.ClientSamples.Core
{
    [ClientSample(CoreConstants.AreaName, CoreConstants.ProjectsRouteName)]
    public class ProjectsSample: ClientSample
    {
        public ProjectsSample(ClientSampleContext context) : base(context)
        {
        }

        /// <summary>
        /// Returns all team projects.
        /// </summary>
        /// <returns></returns>
        [ClientSampleMethod]
        public void ListAllProjectsAndTeams()
        {
            VssConnection connection = Context.Connection;
            ProjectHttpClient projectClient = connection.GetClient<ProjectHttpClient>();
            TeamHttpClient teamClient = connection.GetClient<TeamHttpClient>();

            IEnumerable<TeamProjectReference> projects = projectClient.GetProjects().Result;

            foreach(var project in projects)
            {
                Context.Log("Teams for project {0}:", project.Name);
                Context.Log("--------------------------------------------------");

                IEnumerable<WebApiTeam> teams = teamClient.GetTeamsAsync(project.Name).Result;
                foreach (var team in teams)
                {
                    Context.Log(" {0}: {1}", team.Name, team.Description);
                }
            }
        }

        /// <summary>
        /// Returns only team projects that have the specified state.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        [ClientSampleMethod]
        public IEnumerable<TeamProjectReference> GetProjectsByState(ProjectState state = ProjectState.All)
        {
            VssConnection connection = Context.Connection;
            ProjectHttpClient projectClient = connection.GetClient<ProjectHttpClient>();

            IEnumerable<TeamProjectReference> projects = projectClient.GetProjects(state).Result;

            return projects;
        }

        [ClientSampleMethod]
        public TeamProjectReference GetProjectDetails(string projectName = "Fabrikam")
        {
            VssConnection connection = Context.Connection;
            ProjectHttpClient projectClient = connection.GetClient<ProjectHttpClient>();

            TeamProject project = projectClient.GetProject(projectName, includeCapabilities: true, includeHistory: true).Result;

            return project;                    
        }

        [ClientSampleMethod]
        public OperationReference CreateProject(string name = "Fabrikam", string processName = "Agile")
        {
            // Setup version control properties
            Dictionary<string, string> versionControlProperties = new Dictionary<string, string>();

            versionControlProperties[TeamProjectCapabilitiesConstants.VersionControlCapabilityAttributeName] = 
                SourceControlTypes.Git.ToString();

            // Setup process properties       
            ProcessHttpClient processClient = Context.Connection.GetClient<ProcessHttpClient>();
            Guid processId = processClient.GetProcessesAsync().Result.Find(process => { return process.Name.Equals(processName, StringComparison.InvariantCultureIgnoreCase); }).Id;

            Dictionary<string, string> processProperaties = new Dictionary<string, string>();

            processProperaties[TeamProjectCapabilitiesConstants.ProcessTemplateCapabilityTemplateTypeIdAttributeName] =
                processId.ToString();

            // Construct capabilities dictionary
            Dictionary<string, Dictionary<string, string>> capabilities = new Dictionary<string, Dictionary<string, string>>();

            capabilities[TeamProjectCapabilitiesConstants.VersionControlCapabilityName] = 
                versionControlProperties;
            capabilities[TeamProjectCapabilitiesConstants.ProcessTemplateCapabilityName] = 
                processProperaties;

            TeamProject projectCreateParameters = new TeamProject()
            {
                Name = name,
                Description = "My project description",
                Capabilities = capabilities
            };

            VssConnection connection = Context.Connection;
            ProjectHttpClient projectClient = connection.GetClient<ProjectHttpClient>();

            OperationReference createProjectOperationStatus = projectClient.QueueCreateProject(projectCreateParameters).Result;

            // TODO: check operation status and wait for it to complete before returning the new project

            return createProjectOperationStatus;
        }

        public OperationReference GetOperationStatus(Guid operationId)
        {
            VssConnection connection = Context.Connection;
            OperationsHttpClient operationsClient = connection.GetClient<OperationsHttpClient>();

            OperationReference operationStatus = operationsClient.GetOperation(operationId).Result;

            return operationStatus;
        }

        [ClientSampleMethod]
        public OperationReference RenameProject(String currentName = "Fabrikam", string newName = "Fabrikam (renamed)")
        {
            VssConnection connection = Context.Connection;
            ProjectHttpClient projectClient = connection.GetClient<ProjectHttpClient>();

            Guid projectId = projectClient.GetProject(currentName).Result.Id;

            TeamProject updatedTeamProject = new TeamProject()
            {
                Name = newName
            };

            OperationReference operationStatus = projectClient.UpdateProject(projectId, updatedTeamProject).Result;

            return operationStatus;
        }

        [ClientSampleMethod]
        public OperationReference ChangeProjectDescription(string projectName = "Fabrikam", string newDescription = "New description for Fabrikam")
        {
            TeamProject updatedTeamProject = new TeamProject()
            {
                Description = newDescription
            };

            VssConnection connection = Context.Connection;
            ProjectHttpClient projectClient = connection.GetClient<ProjectHttpClient>();

            Guid projectId = projectClient.GetProject(projectName).Result.Id;

            OperationReference operationStatus = projectClient.UpdateProject(projectId, updatedTeamProject).Result;

            return operationStatus;
        }

        public OperationReference DeleteTeamProject(Guid projectId)
        {
            VssConnection connection = Context.Connection;
            ProjectHttpClient projectClient = connection.GetClient<ProjectHttpClient>();
           
            OperationReference operationStatus = projectClient.QueueDeleteProject(projectId).Result;

            return operationStatus;
        }
    }
}