using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.CompilerServices;

namespace XmlCupomSat
{
    public partial class Form1 : Form
    {
        private SqlConnection con;

        #region Metodos Privados
        #endregion
        private void conexao(string instancia, string banco, string usuario, string senha)
        {
            con = new SqlConnection(string.Format(
                    @"Data Source = {0};Initial Catalog={1};Persist Security Info=True;User ID={2};Password={3}", instancia, banco, usuario, senha));
            try
            {
                con.Open();
            }
            catch (Exception er)
            {

                MessageBox.Show(er.Message);
            }

        }
        private void GravaArquivo(string pastaSalva, string nomeArquivo, string xml)
        {
            try
            {
                //Escreve ou cria Arquivo
                string EndAplicacao = pastaSalva + string.Format(@"\CF-e{0}.XML", nomeArquivo);
                StreamWriter ArquivoConfig = new StreamWriter(EndAplicacao);
                string Dados = string.Format("{0}", xml);
                ArquivoConfig.WriteLine(Dados);
                ArquivoConfig.Close();
            }
            catch (Exception Erro)
            {
                MessageBox.Show(Erro.Message);
            }
        }
        private void ExecutaGrava(string serie, DateTime dataInicio, DateTime dataFim)
        {
            string dataIni = dataInicio.Date.ToString("yyyyMMdd");
            string dataF = dataFim.Date.ToString("yyyyMMdd");
            SqlCommand dc = new SqlCommand();
            try
            {

                dc.CommandText = string.Format(@"select nc.serie, nc.status, nc.xmlaprova, nc.xmlcancela, nc.numero 
from NFiscal as nf inner join Nfiscalcontrole nc on nf.NumNF = nc.Numero and nf.Series = nc.Serie
where nc.Serie = '{0}' and nf.DataNota between '{1}' and '{2} 23:59:59:999'", serie, dataIni, dataF);
                dc.CommandType = CommandType.Text;
                dc.Connection = con;
            }
            catch (Exception z)
            {
                MessageBox.Show(z.Message);
            }

            SqlDataReader rd;
            rd = dc.ExecuteReader();


            while (rd.Read())
            {
                if (rd["status"].ToString() == "C" && rd["XmlCancela"].ToString() != "")
                {
                    GravaArquivo(txtCaminho.Text, "Canc"+ rd["Numero"].ToString(), rd["XmlCancela"].ToString());
                }
                else if (rd["status"].ToString() == "I" && rd["XmlAprova"].ToString() != "")
                {
                    GravaArquivo(txtCaminho.Text, rd["Numero"].ToString(), rd["XmlAprova"].ToString());
                }
            }
            MessageBox.Show("Importação Finalizada com sucesso!");
        }
        private void FechaConexao()
        {
            con.Close();
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            #region Verifica parametros Banco
            if (txtInstancia.Text != "")
            {
                if (txtBanco.Text != "")
                {
                    if (txtId.Text != "")
                    {
                        conexao(txtInstancia.Text, txtBanco.Text, txtId.Text, txtSenha.Text);
                    }
                    else
                    {
                        MessageBox.Show("Informe o Usuario");
                        txtId.Focus();
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("Informe a Base de Dados");
                    txtBanco.Focus();
                    return;
                }
            }
            else
            {
                MessageBox.Show("Informe o caminnho da Base de Dados");
                txtInstancia.Focus();
                return;
            }
            #endregion
            #region Verifica conexão aberta e se o caminho destino foi informado
            if (con.State == ConnectionState.Open)
            {
                if (txtCaminho.Text != "")
                {
                    ExecutaGrava(txtSerie.Text, dtpDataDe.Value, dtpDataAte.Value);
                }
                else
                {
                    txtCaminho.Focus();
                    return;
                }
            }
            else
            {
                MessageBox.Show("Erro na conexão com a Base de Dados");
            }
            #endregion
        }



        #region Caminho da pasta
        #endregion
        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog pasta = new FolderBrowserDialog();
            if (pasta.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtCaminho.Text = pasta.SelectedPath;
            }
        }
    }
}
