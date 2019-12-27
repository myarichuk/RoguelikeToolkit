using System.IO;
using System.Linq;

namespace RoguelikeToolkit.Entities
{
    public class EntityTemplateContainer
    {

        public void LoadTemplate(string filename)
        {
            if(!File.Exists(filename))
                throw new FileNotFoundException("Couldn't find template file.", filename);
        }
    }
}
