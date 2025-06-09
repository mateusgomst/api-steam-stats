using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APISTEAMSTATS.models;
using APISTEAMSTATS.repository;
using APISTEAMSTATS.services;

public class DailyTaskService
{
    private readonly GameService _gameService;
    private readonly WishGameRepository _wishGameRepository;
    private readonly GameRepository _gameRepository;
    private readonly EmailAcl _emailAcl;
    private readonly UserRepository _userRepository;

    public DailyTaskService(GameService gameService, WishGameRepository wishGameRepository,
        GameRepository gameRepository, EmailAcl emailAcl, UserRepository userRepository)
    {
        _gameService = gameService;
        _wishGameRepository = wishGameRepository;
        _gameRepository = gameRepository;
        _emailAcl = emailAcl;
        _userRepository = userRepository;
    }

    public async Task<bool> ExecuteDailyTask()
    {
        try
        {
            Console.WriteLine("[INFO] Iniciando tarefa diária de verificação de promoções...");

            // Primeiro atualiza a lista de jogos
            var (uploadSuccess, uploadError) = await _gameService.UploadAllGames();

            if (!uploadSuccess)
            {
                Console.WriteLine($"[ERROR] Falha ao atualizar lista de jogos: {uploadError}");
                return false;
            }

            List<WishGame> wishList = await _wishGameRepository.GetAllWishList();

            if (!wishList.Any())
            {
                Console.WriteLine("[INFO] Nenhum jogo encontrado na wish list dos usuários.");
                return true;
            }

            int emailsEnviados = 0;
            int emailsFalharam = 0;
            var errosDetalhados = new List<string>();

            foreach (WishGame wish in wishList)
            {
                try
                {
                    int appId = wish.AppId;
                    int wishDiscount = wish.Discount;

                    Game game = await _gameRepository.FindGameByAppidPrimaryKey(appId);

                    if (game == null)
                    {
                        Console.WriteLine($"[WARN] Jogo com ID {appId} não encontrado na base de dados.");
                        continue;
                    }

                    if (game.Discount > wishDiscount)
                    {
                        User user = await _userRepository.FindUserById(wish.UserId);

                        if (user == null)
                        {
                            Console.WriteLine($"[WARN] Usuário com ID {wish.UserId} não encontrado.");
                            continue;
                        }

                        var (success, errorMessage) = await _emailAcl.SendPromotionEmail(
                            toEmail: user.Login,
                            gameName: game.NameGame,
                            discount: game.Discount,
                            appid: game.AppId
                        );

                        if (success)
                        {
                            emailsEnviados++;
                            Console.WriteLine(
                                $"[SUCCESS] E-mail enviado para {user.Login} - Jogo: {game.NameGame} ({game.Discount}% desconto)");
                        }
                        else
                        {
                            emailsFalharam++;
                            string erro =
                                $"Falha ao enviar e-mail para {user.Login} - Jogo: {game.NameGame}. Motivo: {errorMessage}";
                            errosDetalhados.Add(erro);
                            Console.WriteLine($"[ERROR] {erro}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    emailsFalharam++;
                    string erro =
                        $"Erro ao processar wish do usuário {wish.UserId} para o jogo {wish.AppId}: {ex.Message}";
                    errosDetalhados.Add(erro);
                    Console.WriteLine($"[ERROR] {erro}");
                }
            }

            // Relatório final
            if (emailsEnviados == 0 && emailsFalharam == 0)
            {
                Console.WriteLine("[INFO] Nenhum jogo em promoção encontrado nas wish lists dos usuários.");
            }
            else
            {
                Console.WriteLine($"[INFO] Relatório da tarefa diária:");
                Console.WriteLine($"  • E-mails enviados com sucesso: {emailsEnviados}");
                Console.WriteLine($"  • E-mails que falharam: {emailsFalharam}");

                if (errosDetalhados.Any())
                {
                    Console.WriteLine("[ERROR] Detalhes dos erros:");
                    foreach (var erro in errosDetalhados)
                    {
                        Console.WriteLine($"  • {erro}");
                    }
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CRITICAL] Erro crítico na execução da tarefa diária: {ex.Message}");
            Console.WriteLine($"[CRITICAL] Stack trace: {ex.StackTrace}");

            // Relança a exceção para que o job scheduler saiba que houve falha
            throw new InvalidOperationException("Falha crítica na tarefa diária de verificação de promoções", ex);
        }
    }
}