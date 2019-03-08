using System;
using System.Reactive;
using System.Reactive.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace AcadLib.Reactive
{
    public class DocumentsEvents
    {
        private readonly DocumentCollection docMan;

        public DocumentsEvents(DocumentCollection docMan)
        {
            this.docMan = docMan;
        }
        
        public IObservable<EventPattern<DocumentLockModeChangedEventArgs>> LockModeChanged =>
            Observable.FromEventPattern<DocumentLockModeChangedEventHandler, DocumentLockModeChangedEventArgs>
                (x => docMan.DocumentLockModeChanged += x, x => docMan.DocumentLockModeChanged -= x);
    }
}