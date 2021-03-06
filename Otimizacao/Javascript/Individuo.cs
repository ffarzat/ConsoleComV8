﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otimizacao.Javascript
{
    /// <summary>
    /// Representa um individuo na Iteração
    /// </summary>
    public class Individuo
    {
        /// <summary>
        /// Id do Individuo
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Código final do individuo
        /// </summary>
        public string Codigo { get; set; }

        /// <summary>
        /// Arvore abstrata do individuo
        /// </summary>
        public string Ast { get; set; }

        /// <summary>
        /// Valor de Fitness calculado ao final
        /// </summary>
        public double Fitness { get; set; }

        /// <summary>
        /// Caminho do arquivo do inidviduo
        /// </summary>
        public string Arquivo { get; set; }

        /// <summary>
        /// Define como o individuo foi criado
        /// </summary>
        public Operador CriadoPor { get; set; }

        /// <summary>
        /// Número de testes que passaram com sucesso
        /// </summary>
        public int TestesComSucesso { get; set; }

        /// <summary>
        /// Tempo que o individuo levou
        /// </summary>
        public string TempoExecucao { get; set; }

        /// <summary>
        /// Construtor Default
        /// </summary>
        public Individuo()
        {
            Id = Guid.NewGuid();
            Fitness = Int64.MaxValue;
            CriadoPor = Operador.Clonagem;
            TestesComSucesso = 0;
            TempoExecucao = "00:00:00.0000";
            Codigo = "";
        }
        /// <summary>
        /// Gera um clone do individuo atual
        /// </summary>
        /// <returns></returns>
        public Individuo Clone()
        {
            return new Individuo()
                {
                    Ast = this.Ast,
                };
        }


    }

    /// <summary>
    /// Operação Genética
    /// </summary>
    public enum Operador
    {
        Mutacao,
        Cruzamento,
        Clonagem
    }

}
