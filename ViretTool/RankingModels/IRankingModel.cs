using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.RankingModels {
    interface IRankingModel {
        /// <summary>
        /// Fired when a model changes ranking.
        /// </summary>
        event RankingChangedHandler RankingChangedEvent;
        /// <summary>
        /// Fired when a model starts recalculating results.
        /// </summary>
        event RankingInvalidatedHandler RankingInvalidatedEvent;
        /// <summary>
        /// Return last result.
        /// </summary>
        List<SimilarityModels.RankedFrame> LastResult { get; }
        /// <summary>
        /// Get user readable name of the model.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Show message box with an error.
        /// </summary>
        event MessageReporterHandler MessageReporterEvent;
    }

    delegate void MessageReporterHandler(IRankingModel self, MessageType type, string message);

    enum MessageType { Information, Exception }

    delegate void RankingChangedHandler(IRankingModel self);
    delegate void RankingInvalidatedHandler(IRankingModel self);
}
