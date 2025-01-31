using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HttpValidacaoCpf
{
    public static class FnValidarCpf
    {
        [FunctionName("FnValidarCpf")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Iniciando a validação do CPF.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            if (data == null || data.cpf == null)
            {
                return new BadRequestObjectResult("Por favor, informe o CPF.");
            }

            string cpf = data.cpf.ToString();

            // Valida o CPF
            bool cpfValido = ValidaCPF(cpf);

            if (cpfValido)
            {
                return new OkObjectResult($"CPF {cpf} é válido.");
            }
            else
            {
                return new BadRequestObjectResult($"CPF {cpf} é inválido.");
            }
        }

        // Função para validar CPF
        public static bool ValidaCPF(string cpf)
        {
            // Remover caracteres não numéricos
            cpf = cpf.Trim().Replace(".", "").Replace("-", "");

            // Verifica se o CPF tem 11 caracteres
            if (cpf.Length != 11 || cpf.All(c => c == cpf[0]))
            {
                return false; // CPF inválido se todos os números forem iguais
            }

            // Valida os dígitos verificadores
            int[] multiplicador1 = new int[] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            string tempCpf = cpf.Substring(0, 9);
            string digito = cpf.Substring(9, 2);

            // Calcula o primeiro dígito verificador
            int soma = 0;
            for (int i = 0; i < 9; i++)
            {
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];
            }

            int resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            string digito1 = resto.ToString();
            tempCpf = tempCpf + digito1;

            // Calcula o segundo dígito verificador
            soma = 0;
            for (int i = 0; i < 10; i++)
            {
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];
            }

            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            string digito2 = resto.ToString();

            // Verifica se os dígitos verificadores calculados são iguais aos do CPF informado
            return digito == digito1 + digito2;
        }
    }
}
