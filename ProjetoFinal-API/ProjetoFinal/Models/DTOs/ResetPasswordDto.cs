using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ProjetoFinal.Models.DTOs
{
    public class ResetPasswordDto
    {
        public string Email { get; set; } = null!;
    }
}

