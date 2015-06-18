using JetBrains.Annotations;

namespace SQLite.Net
{
	[PublicAPI]
	public class JoinResult<TOuter, TInner> 
        where TInner: class 
        where TOuter: class
	{
        private readonly IContractResolver _resolver;
        private TInner _inner;
        private TOuter _outer;

        public JoinResult(IContractResolver resolver)
        {
            _resolver = resolver;
        }

		public TInner Inner
        {
            get { return _inner ?? (_inner = (TInner)_resolver.CreateObject(typeof(TInner))); }
        }

		public TOuter Outer
        {
            get { return _outer ?? (_outer = (TOuter)_resolver.CreateObject(typeof(TOuter))); }
        }
	}
}