using analyzer;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Antlr4.Runtime.Misc;
using System.Text.Json;
using System.Text;

namespace api.Controllers
{
    [Route("[controller]")]
    public class Compile : Controller
    {
        private readonly ILogger<Compile> _logger;

        public Compile(ILogger<Compile> logger)
        {
            _logger = logger;
        }

        public class CompileRequest
        {
            [Required]
            public required string code { get; set; }
        }


        // POST /compile
        [HttpPost]
        public IActionResult Post([FromBody] CompileRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { error = "Invalid request" });
            }

            var inputStream = new AntlrInputStream(request.code);
            var lexer = new LanguageLexer(inputStream);
            var tokens = new CommonTokenStream(lexer);
            var parser = new LanguageParser(tokens);

            lexer.RemoveErrorListeners();
            lexer.AddErrorListener(new LexicoErrorListener());

            parser.RemoveErrorListeners();
            parser.AddErrorListener(new SintacticErrorListener());

            try
            {
                var tree = parser.program();
                
                var interpreter = new InterpreterVisitor();
                interpreter.Visit(tree);
                var compiler = new CompileVisitor();
                compiler.Visit(tree);
                return Ok(new { result = compiler.c.ToString() });
            }
            catch (ParseCanceledException e)
            {
                return BadRequest(new { error = e.Message });
            }
            catch (SemanticError e)
            {

                return BadRequest(new { error = e.Message });
            }
        }

         // POST / compile/ast
        [HttpPost("ast")]
        public async Task<IActionResult> GetAst([FromBody] CompileRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { error = "Invalid request" });
            }

            string grammarPath = Path.Combine(Directory.GetCurrentDirectory(), "Language.g4");
            
            var grammar = "";
            try{
                if (System.IO.File.Exists(grammarPath))
                {
                    grammar =  await System.IO.File.ReadAllTextAsync(grammarPath);
                }
                else
                {
                    return BadRequest(new { error = "Grammar file not found" });
                }
            }catch (SystemException)
            {
                return BadRequest(new { error = "Error reading grammar file" });
            }

            var payload = new
            {
                grammar,
                lexgrammar = "",
                input = request.code,
                start = "program",
            };

            var json = JsonSerializer.Serialize(payload);
            var context = new StringContent(json, Encoding.UTF8, "application/json");
            using(var client = new HttpClient())
            {
                try{
                    HttpResponseMessage response = await client.PostAsync("http://lab.antlr.org/parse/", context);
                    response.EnsureSuccessStatusCode();

                    string result = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(result);
                    var root = doc.RootElement;

                    if(root.TryGetProperty("result", out JsonElement resultElement) && resultElement.TryGetProperty("svgtree", out JsonElement svgtreeElement))
                    {
                        string svgtree = svgtreeElement.GetString() ?? string.Empty;
                        return Content(svgtree, "image/svg+xml");
                    }

                    return BadRequest(new { error = "Error parsing response"});

                }catch (System.Exception ex)
                {
                    _logger.LogError(ex, "Error creating HttpClient");
                    return BadRequest(new { error = "Error creating HttpClient" });
                }
            }
        }
    }
}
