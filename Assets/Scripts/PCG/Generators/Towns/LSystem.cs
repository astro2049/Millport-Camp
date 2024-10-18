using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PCG.Generators.Towns
{
    [CreateAssetMenu(fileName = "LSystem", menuName = "Procedural Generation/L-System")]
    public class LSystem : ScriptableObject
    {
        public string axiom;
        public List<Rule> rules;
        public int iterations;
        public float angle;

        private Dictionary<char, List<string>> ruleDict;

        private void InitializeRuleDict()
        {
            ruleDict = new Dictionary<char, List<string>>();
            foreach (Rule rule in rules) {
                ruleDict.Add(rule.symbol, rule.results);
            }
        }

        public string GenerateSentence()
        {
            InitializeRuleDict();
            return GenerateSentence(axiom, 0);
        }

        private string GenerateSentence(string current, int round)
        {
            if (round == iterations) {
                return current;
            }
            round++;

            StringBuilder sb = new StringBuilder();
            foreach (char c in current) {
                if (ruleDict.ContainsKey(c)) {
                    List<string> possibleResults = ruleDict[c];
                    string selectedResult = possibleResults[Random.Range(0, possibleResults.Count)];
                    sb.Append(selectedResult);
                } else {
                    sb.Append(c);
                }
            }

            return GenerateSentence(sb.ToString(), round);
        }
    }

    [Serializable]
    public class Rule
    {
        public char symbol;
        public List<string> results;
    }
}
