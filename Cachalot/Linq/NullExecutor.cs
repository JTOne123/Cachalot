using System.Collections.Generic;
using System.Linq;
using Client.Messages;
using Client.Queries;
using Remotion.Linq;

namespace Cachalot.Linq
{
    public class NullExecutor : IQueryExecutor
    {
        private readonly TypeDescription _typeDescription;

        public NullExecutor(TypeDescription typeDescription)
        {
            _typeDescription = typeDescription;
        }

        public OrQuery Expression { get; private set; }

        public T ExecuteScalar<T>(QueryModel queryModel)
        {
            return default;
        }

        public T ExecuteSingle<T>(QueryModel queryModel, bool returnDefaultWhenEmpty)
        {
            return default;
        }

        public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel)
        {
            var visitor = new QueryVisitor(_typeDescription);

            visitor.VisitQueryModel(queryModel);

            Expression = visitor.RootExpression;

            return Enumerable.Empty<T>();
        }
    }
}