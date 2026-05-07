using System.Text.Json;
using Dizimo.Models;
using Microsoft.Extensions.Logging;

namespace Dizimo.Data
{
    public class SeedDataService
    {
        private readonly ProjectRepository _projectRepository;
        private readonly TaskRepository _taskRepository;
        private readonly TagRepository _tagRepository;
        private readonly CategoryRepository _categoryRepository;
        private readonly string _seedDataFilePath;
        private readonly ILogger<SeedDataService> _logger;

        public SeedDataService(ProjectRepository projectRepository, TaskRepository taskRepository, TagRepository tagRepository, CategoryRepository categoryRepository, ILogger<SeedDataService> logger)
        {
            _projectRepository = projectRepository;
            _taskRepository = taskRepository;
            _tagRepository = tagRepository;
            _categoryRepository = categoryRepository;
            _logger = logger;
            
            var appPath = AppContext.BaseDirectory;
            _seedDataFilePath = Path.Combine(appPath, "SeedData.json");
        }

        public async Task LoadSeedDataAsync()
        {
            ClearTables();

            if (!File.Exists(_seedDataFilePath))
            {
                _logger.LogWarning($"Arquivo de seed data não encontrado: {_seedDataFilePath}");
                return;
            }

            ProjectsJson? payload = null;
            try
            {
                await using var fileStream = File.OpenRead(_seedDataFilePath);
                payload = JsonSerializer.Deserialize(fileStream, JsonContext.Default.ProjectsJson);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error deserializing seed data");
                return;
            }

            try
            {
                if (payload?.Projects != null)
                {
                    foreach (var project in payload.Projects)
                    {
                        if (project == null)
                        {
                            continue;
                        }

                        if (project.Category != null)
                        {
                            await _categoryRepository.SaveItemAsync(project.Category);
                            project.CategoryID = project.Category.ID;
                        }

                        await _projectRepository.SaveItemAsync(project);

                        if (project.Tasks != null)
                        {
                            foreach (var task in project.Tasks)
                            {
                                task.ProjectID = project.ID;
                                await _taskRepository.SaveItemAsync(task);
                            }
                        }

                        if (project.Tags != null)
                        {
                            foreach (var tag in project.Tags)
                            {
                                await _tagRepository.SaveItemAsync(tag, project.ID);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error saving seed data");
                throw;
            }
        }

        private async void ClearTables()
        {
            try
            {
                await Task.WhenAll(
                    _projectRepository.DropTableAsync(),
                    _taskRepository.DropTableAsync(),
                    _tagRepository.DropTableAsync(),
                    _categoryRepository.DropTableAsync());
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"[ERRO] Erro ao limpar tabelas: {e.Message}");
            }
        }
    }
}