using NetLib.Monad;

namespace AcadLib.Reactive
{
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using Autodesk.AutoCAD.DatabaseServices;

    public class DboEvents
    {
        private readonly DBObject _dbo;

        public DboEvents(DBObject dbo)
        {
            _dbo = dbo;
        }

        public IObservable<EventPattern<EventArgs>> Modified => Observable.FromEventPattern<EventHandler, EventArgs>(
            x => _dbo.Try(d => d.Modified += x),
            x => _dbo.Try(d => d.Modified -= x));

        public IObservable<EventPattern<ObjectEventArgs>> Copied =>
            Observable.FromEventPattern<ObjectEventHandler, ObjectEventArgs>(
                x => _dbo.Try(d => d.Copied += x),
                x => _dbo.Try(d => d.Copied -= x));

        public IObservable<EventPattern<ObjectErasedEventArgs>> Erased =>
            Observable.FromEventPattern<ObjectErasedEventHandler, ObjectErasedEventArgs>(
                x => _dbo.Try(d => d.Erased += x),
                x => _dbo.Try(d => d.Erased -= x));

        public IObservable<EventPattern<ObjectClosedEventArgs>> ObjectClosed => Observable
            .FromEventPattern<ObjectClosedEventHandler, ObjectClosedEventArgs>(
                x => _dbo.Try(d => d.ObjectClosed += x),
                x => _dbo.Try(d => d.ObjectClosed -= x));
    }
}
