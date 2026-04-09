/*
Entrega: permitir abertura de viagem sem informar peso carregado.

Impacto em banco:
- Nenhuma alteracao estrutural.
- Nenhuma migration nova foi criada para esta entrega.

Situacao:
- A coluna [trips].[LoadedWeightTons] ja existe.
- A regra passou a ser tratada apenas em codigo (frontend + backend).

Acao em PRD:
- Nao executar DDL para esta entrega.
- Publicar apenas a aplicacao.
*/

PRINT 'Nenhum script SQL precisa ser executado em PRD para a entrega 2026-04-08-prd-trip-open-without-loaded-weight.';
GO
