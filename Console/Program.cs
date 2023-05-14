using easyJSON;

namespace ConsoleTest {

    public class Testj2 {
        public string? Text;
        public long? Num;
        public bool? IsRight;
        public string[]? arr;
    }

    public class Testj {
        public string? Text;
        public long? Num;
        public bool? IsRight;
        public Testj2[]? Testj2;
    }
    internal class Program {
        static void Main(string[] args) {
            string json = "[  {    \"num\": -7018,    \"text\": \"nostrud\",    \"IsRight\": true,    \"Testj2\": [      {        \"num\": 1350,        \"text\": \"in\",        \"IsRight\": true,        \"arr\": [          \"dolor\",          \"veniam\",          \"ad\"        ]      },      {        \"num\": 155,        \"text\": \"excepteur\",        \"IsRight\": false,        \"arr\": [          \"duis\",          \"ad\",          \"aliquip\"        ]      },      {        \"num\": 4089,        \"text\": \"elit\",        \"IsRight\": true,        \"arr\": [          \"reprehenderit\",          \"irure\",          \"id\"        ]      }    ]  },  {    \"num\": 4296,    \"text\": \"magna\",    \"IsRight\": true,    \"Testj2\": [      {        \"num\": 8338,        \"text\": \"esse\",        \"IsRight\": true,        \"arr\": [          \"dolore\",          \"do\",          \"tempor\"        ]      },      {        \"num\": 1859,        \"text\": \"cupidatat\",        \"IsRight\": true,        \"arr\": [          \"ut\",          \"sit\",          \"fugiat\"        ]      },      {        \"num\": 1861,        \"text\": \"ut\",        \"IsRight\": true,        \"arr\": [          \"sint\",          \"occaecat\",          \"commodo\"        ]      }    ]  },  {    \"num\": 1977,    \"text\": \"sit\",    \"IsRight\": true,    \"Testj2\": [      {        \"num\": 494,        \"text\": \"incididunt\",        \"IsRight\": true,        \"arr\": [          \"nulla\",          \"tempor\",          \"nostrud\"        ]      },      {        \"num\": 1129,        \"text\": \"irure\",        \"IsRight\": false,        \"arr\": [          \"veniam\",          \"excepteur\",          \"laboris\"        ]      },      {        \"num\": 2102,        \"text\": \"magna\",        \"IsRight\": false,        \"arr\": [          \"ea\",          \"dolore\",          \"mollit\"        ]      }    ]  }]";
            var result = jsonParser.Deserialize<Testj[]>(json);
            string result2 = jsonParser.Serialize(result);
            var result3 = jsonParser.Deserialize<Testj[]>(result2);
            Console.WriteLine(result2);

        }
    }
}