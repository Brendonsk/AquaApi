using MongoDB.Driver;
using MqttApiPg.Entities;

namespace MqttApiPg.Services
{
    public class MensalService : CollectionService<Mensal>
    {
        private DiariaService _diariaService;
        public MensalService(MongoDbContext context, DiariaService diariaService) : base(context.Mensais)
        {
            _diariaService = diariaService;
        }

        public async Task<Mensal?> GetPorMesEAno(int ano, int mes)
        {
            return await (await collection.FindAsync(x => x.Ano.Equals(ano) && x.Mes.Equals(mes)))
                .FirstOrDefaultAsync();
        }

        public async Task<List<Mensal>> GetByAno(int ano)
        {
            return await (await collection.FindAsync(x => x.Ano.Equals(ano)))
                .ToListAsync();
        }

        public async Task SalvaConsumoMesAnterior()
        {
            DateTime hojeUmMesAtras = DateTime.Now;
            hojeUmMesAtras.AddMonths(-1);
            var diarias = await _diariaService.GetDiariasByMonthOfYear(hojeUmMesAtras.Year, hojeUmMesAtras.Month);
            int numeroRegistros = diarias.Count();

            Diaria? ultimoRegistro = diarias.OrderBy(x => x.DiaHora).FirstOrDefault();
            decimal consumo = (ultimoRegistro?.Valor ?? 0)/1000;

            await this.CreateAsync(new Mensal() { 
                 Ano = hojeUmMesAtras.Year,
                 ConsumoTotal = consumo,
                 Mes = hojeUmMesAtras.Month
            });
        }
    }
}
