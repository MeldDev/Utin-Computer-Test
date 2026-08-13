using Cysharp.Threading.Tasks;

namespace UtinComputerTest.Infrastructure.Progress.SaveParts
{
    public interface IProgressRepository
    {
        public string PartId { get; }
        public int Version { get; }
        public bool AutoSaveEnabled { get; }
        public bool IsDirty { get; }
        public bool IsLoaded { get; }
        public UniTask LoadAsync();
        public UniTask SaveAsync();
    }

    // Маркер группы прогресса, которая резолвится и загружается при инициализации ProjectContext.
    public interface IProjectLoadProgressRepository : IProgressRepository
    {
    }

    // Маркер группы прогресса, которая резолвится и загружается при старте Map scene.
    public interface IMapLoadProgressRepository : IProgressRepository
    {
    }

    // Маркер группы прогресса, которая резолвится и загружается при старте Robbery scene.
    public interface IRobberyLoadProgressRepository : IProgressRepository
    {
    }

    // Маркер группы прогресса, которая резолвится и загружается при старте Fight scene.
    public interface IFightLoadProgressRepository : IProgressRepository
    {
    }
}
