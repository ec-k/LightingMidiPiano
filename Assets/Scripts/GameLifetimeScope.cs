using VContainer;
using VContainer.Unity;

namespace LightingMidiPiano
{
    public class GameLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<ActiveNoteModel>(Lifetime.Singleton);

            builder.RegisterComponentInHierarchy<KeyboardView>();
            builder.RegisterComponentInHierarchy<NoteBarView>();

            builder.RegisterEntryPoint<MidiInputPresenter>();
        }
    }
}
