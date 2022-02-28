using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Xml.Linq;

namespace IATClient
{
    public interface IUri
    {
        Uri Value { get; }
        Uri URI { get; }
        bool IsObservable { get; }
        void Dispose();
    }

    public class UriContainer : IUri
    {
        public Uri Value { get; set; } = null;
        public Uri URI { get; private set; }
        public bool IsObservable { get { return false; } }
        public UriContainer() { }
        public UriContainer(Uri u)
        {
            Value = u;
            URI = u;
        }
        public void Dispose() { }
    }

    public class Subscription : IDisposable
    {
        private ObservableUri Observable;
        private IObserver<Uri> Observer;

        public Subscription(ObservableUri observable, IObserver<Uri> observer)
        {
            Observable = observable;
            Observer = observer;
        }

        public void Dispose()
        {
            Observable.CancelSubscription(Observer);
        }
    }

    public class ObservableUri : IObservable<Uri>, IPackagePart
    {
        public Uri URI { get; set; }
        public String MimeType { get { return "text/xml+observable-uri"; } }
        public Type BaseType { get { return typeof(ObservableUri); } }
        private Uri _Value = null;
        private List<IObserver<Uri>> Observers = new List<IObserver<Uri>>();
        public Uri Value
        {
            get
            {
                return _Value;
            }
            set
            {
                if (_Value == value)
                    return;
                if (_Value != null)
                {
                    List<PackageRelationship> prList = CIAT.SaveFile.GetRelationshipsByType(_Value, BaseType, typeof(DIBase), "owned-by");
                    foreach (var pr in prList)
                    {
                        CIAT.SaveFile.CreateRelationship(BaseType, typeof(DIBase), value, pr.TargetUri, "owned-by");
                        CIAT.SaveFile.DeleteRelationship(_Value, pr.Id);
                        CIAT.SaveFile.CreateRelationship(typeof(DIBase), BaseType, pr.TargetUri, value, "owns");
                        CIAT.SaveFile.DeleteRelationship(_Value, CIAT.SaveFile.GetRelationship(pr.TargetUri, _Value));
                    }
                }
                _Value = value;
                foreach (var observer in Observers)
                    observer.OnNext(value);
            }
        }

        public ObservableUri()
        {
            URI = CIAT.SaveFile.CreatePart(BaseType, typeof(ObservableUri), MimeType, ".xml");
            CIAT.SaveFile.Register(this);
        }

        public ObservableUri(Uri u)
        {
            URI = u;
            Load();
        }

        public void AddOwner(IPackagePart parentPart)
        {
            CIAT.SaveFile.CreateRelationship(parentPart.BaseType, BaseType, parentPart.URI, URI, "owns");
            CIAT.SaveFile.CreateRelationship(BaseType, parentPart.BaseType, URI, parentPart.URI, "owned-by");
            if (Value != null)
                CIAT.SaveFile.GetDI(Value).AddOwner(parentPart.URI);
        }

        public void ReleaseOwner(IPackagePart parentPart)
        {
            CIAT.SaveFile.DeleteRelationship(URI, parentPart.URI);
            CIAT.SaveFile.DeleteRelationship(parentPart.URI, URI);
            if (Value != null)
                CIAT.SaveFile.GetDI(Value).ReleaseOwner(parentPart.URI);
        }

        public IDisposable Subscribe(IObserver<Uri> observer)
        {
            Observers.Add(observer);
            observer.OnNext(Value);
            return new Subscription(this, observer);
        }
        /*
                public void Consume(ObservableUri o)
                {
                    CIAT.SaveFile.TransferRelationships(o.Value, Value);
                    _Value = o.Value;
                    List<Uri> owningUris = CIAT.SaveFile.GetRelationships(o.URI).Where(pr => pr.RelationshipType.EndsWith("owned-by")).Select(pr => pr.TargetUri).ToList();
                    foreach (var u in owningUris)
                    {
                        CIAT.SaveFile.CreateRelationship(BaseType, typeof(DIBase), URI, u, "owned-by");
                        CIAT.SaveFile.DeleteRelationship(o.URI, u);
                        CIAT.SaveFile.CreateRelationship(typeof(DIBase), BaseType, u, URI, "owns");
                        CIAT.SaveFile.DeleteRelationship(u, o.URI);
                    }
                    foreach (var observer in o.Observers)
                    {
                        observer.OnNext(Value);
                    }
                    Observers.AddRange(o.Observers);
                    o.Observers.Clear();
                }
        */
        public void CancelSubscription(IObserver<Uri> o)
        {
            Observers.Remove(o);
        }

        public void Dispose()
        {

            CIAT.SaveFile.DeletePart(URI);
        }

        public void Save()
        {
            XDocument xDoc = new XDocument();
            if (Value != null)
                xDoc.Add(new XElement("ObservableUri", new XElement("UriObserved", Value.ToString())));
            else
                xDoc.Add(new XElement("ObservableUri", new XElement("UriObserved", "NULL")));
            Stream s = CIAT.SaveFile.GetWriteStream(this);
            xDoc.Save(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseWriteStreamLock();
        }

        public void Load()
        {
            Stream s = CIAT.SaveFile.GetReadStream(this);
            XDocument xDoc = XDocument.Load(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseReadStreamLock();
            String uriStr = xDoc.Root.Element("UriObserved").Value;
            if (uriStr == "NULL")
                Value = null;
            else
                Value = new Uri(xDoc.Root.Element("UriObserved").Value, UriKind.Relative);
        }
    }

    public class UriObserver : IObserver<Uri>, IDisposable, IUri
    {
        public bool IsObservable { get { return true; } }
        private IDisposable Subscription;
        public Uri Value { get; private set; }
        public Uri URI { get; set; }
        public UriObserver(ObservableUri oUri)
        {
            Subscription = oUri.Subscribe(this);
            URI = oUri.URI;
        }

        public void OnNext(Uri u)
        {
            Value = u;
        }
        public void OnError(Exception ex) { throw new NotImplementedException(); }
        public void OnCompleted() { throw new NotImplementedException(); }
        public void Dispose()
        {
            Subscription.Dispose();
        }

    }

}
