using JetBrains.Annotations;

namespace SQLite.Net
{
	[PublicAPI]
	public class JoinResult<TOuter, TInner>
	{
        public JoinResult(IContractResolver resolver)
        {
            Inner = (TInner)resolver.CreateObject(typeof(TInner));
            Outer = (TOuter)resolver.CreateObject(typeof(TOuter));
        }

		public TInner Inner { get; private set; }

		public TOuter Outer { get; private set; }
	}
}