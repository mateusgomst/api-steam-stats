using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APISTEAMSTATS.models;
using APISTEAMSTATS.repository;
using APISTEAMSTATS.services;

public class DailyTaskService
{
    private readonly GameListService _gameListService;
    private readonly WishListRepository _wishListRepository;
    private readonly GameListRepository _gameListRepository;
    private readonly EmailAcl _emailAcl;
    private readonly UserRepository _userRepository;

    public DailyTaskService(GameListService gameListService, WishListRepository wishListRepository,
        GameListRepository gameListRepository, EmailAcl emailAcl, UserRepository userRepository)
    {
        _gameListService = gameListService;
        _wishListRepository = wishListRepository;
        _gameListRepository = gameListRepository;
        _emailAcl = emailAcl;
        _userRepository = userRepository;
    }

    public async Task<bool> ExecuteDailyTask()
    {
        try
        {
            Console.WriteLine("[INFO] Iniciando tarefa diária de verificação de promoções...");

            // Primeiro atualiza a lista de jogos
            var (uploadSuccess, uploadError) = await _gameListService.UploadAllGames();

            if (!uploadSuccess)
            {
                Console.WriteLine($"[ERROR] Falha ao atualizar lista de jogos: {uploadError}");
                return false;
            }

            List<WishGame> wishList = await _wishListRepository.GetAllWishList();

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
                    int appId = wish.GameId;
                    int wishDiscount = wish.Discount;

                    GameList game = await _gameListRepository.FindGameByAppidPrimaryKey(appId);

                    if (game == null)
                    {
                        Console.WriteLine($"[WARN] Jogo com ID {appId} não encontrado na base de dados.");
                        continue;
                    }

                    if (game.discount > wishDiscount)
                    {
                        User user = await _userRepository.FindUserById(wish.UserId);

                        if (user == null)
                        {
                            Console.WriteLine($"[WARN] Usuário com ID {wish.UserId} não encontrado.");
                            continue;
                        }

                        var (success, errorMessage) = await _emailAcl.SendPromotionEmail(
                            toEmail: user.login,
                            gameName: game.nameGame,
                            discount: game.discount,
                            appid: game.appId
                        );

                        if (success)
                        {
                            emailsEnviados++;
                            Console.WriteLine(
                                $"[SUCCESS] E-mail enviado para {user.login} - Jogo: {game.nameGame} ({game.discount}% desconto)");
                        }
                        else
                        {
                            emailsFalharam++;
                            string erro =
                                $"Falha ao enviar e-mail para {user.login} - Jogo: {game.nameGame}. Motivo: {errorMessage}";
                            errosDetalhados.Add(erro);
                            Console.WriteLine($"[ERROR] {erro}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    emailsFalharam++;
                    string erro =
                        $"Erro ao processar wish do usuário {wish.UserId} para o jogo {wish.GameId}: {ex.Message}";
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