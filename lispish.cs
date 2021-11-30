using System;
using System.Collections.Generic;

public class LispishParser
{
    public enum Symbol
    {
        INT, REAL, STRING, ID, LITERAL, WS, Program, SExpr, List, Seq, Atom
    }

    public class Node
    {
        public Symbol symbol;
        public string text;
        public List<Node> children = null;

        public Node(Symbol symbol, string text) {
            this.symbol = symbol;
            this.text = text;
            this.children = new List<Node>();
        }
		
		public Node(Symbol symbol, string text,Node child) {
            this.symbol = symbol;
            this.text = text;
            this.children = new List<Node>();
            children.Add(child);
        }
		
		public Node(Symbol symbol, string text,List<Node> children) {
            this.symbol = symbol;
            this.text = text;
            this.children = new List<Node>();
            foreach (Node child in children) {
                this.children.Add(child);
            }
        }

        public Node(Symbol symbol, string text, params Node[] children) {
            this.symbol = symbol;
            this.text = text;
            this.children = new List<Node>();
            foreach (Node child in children) {
                this.children.Add(child);
            }
        }

        public void Print(string prefix = "")
        {
			int tab = 42 - prefix.Length;

            if (text == "") {
                Console.WriteLine(prefix + Enum.GetName(typeof(Symbol), symbol));
            } else {
				string s = String.Format("{0}{1," + -tab + "}{2}", prefix, Enum.GetName(typeof(Symbol), this.symbol), this.text);
                Console.WriteLine(s);
            }
            foreach (Node child in children) {
                if (child != null) {
                    child.Print(prefix + "  ");
                }
            }
        }
    }

    static public List<Node> Tokenize(String src)
    {
		src = src + " ";
        List<Node> result = new List<Node>();
        System.Text.StringBuilder lexeme = new System.Text.StringBuilder();

        int state = 0; //start
        int next = 0;
        bool consume = true;

        // Symbol is ttypes[state] when we find the token
        Symbol[] ttypes = {Symbol.WS,           //0
                           Symbol.LITERAL,      //1
                           Symbol.INT,          //2
                           Symbol.ID,           //3  +      
                           Symbol.ID,           //4  -
                           Symbol.REAL,         //5
                           Symbol.ID,           //6  *
                           Symbol.ID,           //7  /
                           Symbol.ID,           //8  =
                           Symbol.ID,           //9  >
                           Symbol.ID,           //10 <
                           Symbol.ID,           //11 any
                           Symbol.STRING, 		//12
						   Symbol.STRING, 		//13
						   Symbol.STRING, 		//14
                          };
        for (int i = 0; i < src.Length;) {
            consume = true;
			char c = src[i];

            switch (state) 
            {
                case 0: 
                    switch (c)
                    {
                        case '(':
                            next = 1;
                            break;
						case ')':
                            next = 1;
                            break;
                        case '+':
                            next = 3;
                            break;
                        case '-':
                            next = 4;
                            break;
                        case '.':
                            next = 5;
                            break;
                        case '*':
                            next = 6;
                            break;
                        case '/':
                            next = 7;
                            break;
                        case '=':
                            next = 8;
                            break;
                        case '>':
                            next = 9;
                            break;
                        case '<':
                            next = 10;
                            break;
                        case '"':
                            next = 12;
                            break;
                        case char cc when Char.IsDigit(cc):
                            next = 2;
                            break;
                        case char cc when Char.IsLetter(cc):
                            next = 11;
                            break;
                        case char cc when Char.IsWhiteSpace(cc):
                            next = 0;
                            break;
                        default:
                            throw new Exception();
                    }
                    break;

                case 1: 
                    next = 0;
                    consume = false;
                    break;

                case 2: 
                    switch (c)
                    {
                        case '.':
                            next = 5;
                            break;
                        case char cc when Char.IsDigit(cc):
                            next = 2;
                            break;
                        default:
                            next = 0;
                            consume = false;
                            break;
                    }
                    break;

                case 3: 
                case 4: 
                    switch (c)
                    {
                        case '.':
                            next = 5;
                            break;
                        case char cc when Char.IsDigit(cc):
                            next = 2;
                            break;
                        default:
                            next = 0;
							consume = false;
                            break;
                    }
                    break;
                case 5: 
                    switch (c)
                    {
                        case char cc when Char.IsDigit(cc):
                            next = 5;
                            break;
                        default:
                            next = 0;
                            consume = false;
                            break;
                    }
                    break;
                case 6: case 7: case 8: case 9: case 10:
                    next = 0;
                    consume = false;
                    break;
                case 11: 
                    switch (c)
                    {
                        case char cc when Char.IsDigit(cc):
                            next = 11;
                            break;
                        case char cc when Char.IsLetter(cc):
                            next = 11;
                            break;
                        default:
                            next = 0;
                            consume = false;
                            break;
                    }
                    break;
                case 12:
                    switch (c)
                    {
                        case '"':
                            next = 13;
                            break;
						case '\\':
							next = 14;
							break;
                        default:
                            next = 12;
                            break;
                    }
                    break;
				case 13:
					next = 0;
                    consume = false;
                    break;
				case 14:
					switch (c)
                    {
						case '\\':
							next = 14;
							break;
                        default:
                            next = 12;
                            break;
                    }
					break;
            } // switching on state

            // Add to the lexeme OR output a token and reset the lexeme
            // Consume the next character (unless WS or finished a token)
            if (consume) {
				i++;
                if (next != 0) {
                    lexeme.Append(c);
                }
            } else {
                result.Add(new  Node(ttypes[state], lexeme.ToString()));
                lexeme.Clear();
            }

            state = next;

        }
		
		if (next != 0) {
			throw new Exception();
		}

        return result;
    }

    public class Parser {
        public Node[] tokens;
        public Node token;
        public Node Program;
        public int index;

        public Parser(Node[] tokens) {
            this.tokens = tokens;
            this.index = 0;
			this.token = tokens[this.index];
            this.Program = ParseProgram();
        }

        public Node NextToken() {
            Node tok = this.token;	
			index++;
            if (index < tokens.Length) {
                this.token = tokens[index];
            } else {
                this.token = null;
            }
            return tok;
        }

        private Node ParseProgram() {
            List<Node> children = new List<Node>();
            while (this.token != null) {
                children.Add(ParseSExpr());
            }
            return new Node(Symbol.Program, "", children);
        }

        private Node ParseSExpr() {
            if (token.text == "(") {
                return new Node(Symbol.SExpr, "", ParseList());
            } else {
                return new Node(Symbol.SExpr, "", ParseAtom());
            }
        }

        private Node ParseList() {
            Node lparen = NextToken();
            if (this.token.text == ")") {
                return new Node(Symbol.List, "", lparen, NextToken());
            } else {
                Node seq = ParseSeq();
                if (this.token.text == ")") {
                    return new Node(Symbol.List, "", lparen, seq, NextToken());
                } else {
                    throw new Exception();
                }
            }
        }

        private Node ParseSeq() {
            Node se = ParseSExpr();
            if (this.token.text == ")") {
                return new Node(Symbol.Seq, "", se);
            } else {
                Node subseq = ParseSeq();
                return new Node(Symbol.Seq, "", se, subseq);
            }
        }

        private Node ParseAtom() {
            if (Enum.GetName(typeof(Symbol), this.token.symbol) == "LITERAL" ||
                Enum.GetName(typeof(Symbol), this.token.symbol) == "ID" ||
                Enum.GetName(typeof(Symbol), this.token.symbol) == "INT" ||
                Enum.GetName(typeof(Symbol), this.token.symbol) == "REAL" ||
                Enum.GetName(typeof(Symbol), this.token.symbol) == "STRING") {
                    return new Node(Symbol.Atom, "", NextToken());
                } else {
                    throw new Exception();
                }
        }


    }

    static private void CheckString(string lispcode)
    {
        try
        {
            Console.WriteLine(new String('=', 50));
            Console.Write("Input: ");
            Console.WriteLine(lispcode);
            Console.WriteLine(new String('-', 50));

            Node[] tokens = Tokenize(lispcode).ToArray();

            Console.WriteLine("Tokens");
            Console.WriteLine(new String('-', 50));
            foreach (Node node in tokens)
            {
                Console.WriteLine($"{Enum.GetName(typeof(Symbol), node.symbol),-20}" + ": " + $"{node.text}");
            }
            Console.WriteLine(new String('-', 50));

            Parser p = new Parser(tokens);

            Console.WriteLine("Parse Tree");
            Console.WriteLine(new String('-', 50));
            p.Program.Print();
            Console.WriteLine(new String('-', 50));
        }
        catch (Exception)
        {
            Console.WriteLine("Threw an exception on invalid input.");
        }
    }


    public static void Main(string[] args)
    {
        //Here are some strings to test on in 
        //your debugger. You should comment 
        //them out before submitting!

        // CheckString(@"(define foo 3)");
        // CheckString(@"(define foo ""bananas"")");
        // CheckString(@"(define foo ""Say \\""Chease!\\"" "")");
        // CheckString(@"(define foo ""Say \\""Chease!\\)");
        // CheckString(@"(+ 3 4)");      
        // CheckString(@"(+ 3.14 (* 4 7))");
        // CheckString(@"(+ 3.14 (* 4 7)");

        CheckString(Console.In.ReadToEnd());
    }
}

