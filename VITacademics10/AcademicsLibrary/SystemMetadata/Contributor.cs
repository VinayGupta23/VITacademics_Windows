using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Academics.SystemMetadata
{
    public class Contributor
    {
        private readonly string _name;
        private readonly string _roles;
        private readonly string _githubProfileUri;

        public string Name
        { get { return _name; } }
        public string Roles
        { get { return _roles; } }
        public string GithubProfileUri
        { get { return _githubProfileUri; } }

        internal Contributor(string name, string roles, string profileUri)
        {
            _name = name;
            _roles = roles;
            _githubProfileUri = profileUri;
        }
    }
}
