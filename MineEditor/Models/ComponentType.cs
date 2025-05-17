using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineEditor.Models
{
  public enum ComponentType
  {
    externalIncomingEvents,
    externalOutgoingEvents,
    internalEvents,
    states,
    initialState,
    internalTransitionFunction,
    externalTransitionFunction,
    stateOutputFunction,
    transitionOutputFunction,
    timeAdvance,
    stateActivity,
    transitionActivity,
  }
}
