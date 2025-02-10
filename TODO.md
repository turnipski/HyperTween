# TODO:

## MVP:

- [x] Parallel Tween API
- [x] Serial Tween API
  - [x] TweenPlayOnPlay
- [x] Stagger Tween API
- [x] Conflict Detection
- [x] Source generated conflict detection systems
- [x] Sourced generated OnPlay/OnStop Systems?
  - [x] Generate OnPlay/OnStop Components
- [x] Sourced generated Output Systems?
  - [x] Automatic From component handling
  - [x] Interpolation
- [ ] Zero duration tweens should still output
- [x] Hermite Easing
- [ ] Managed transform outputs
  - [ ] Use IJobParallelForTransform
- [x] Optimisation
- [x] Tests
  - [x] Structural Tests
  - [x] Latency Tests
  - [ ] Conflicts
- [x] Take into account timing overshoots
- [x] Managed Invoke Systems
- [x] Managed Output Systems
- [ ] Burst Function Ptr Invoking
- [x] Local Rotation/Scale Outputs
- [ ] Managed Transform Output (with TransformAccessArray)
- [ ] Rename API methods
- [ ] Create HyperTween API entrypoint for default works
- [x] Tween Reuse
  - [x] From component removal
  - [x] TweenParameter Reset
- [ ] Doc comments & auto generated documentation
- [ ] Functional Tween Composition Examples
  - [ ] Add links in documentation
- [ ] Additional TweenBuilder types
  - [x] Entity Manager
  - [ ] ECB Parallel Writer - Need to find a way to deal with sortKey
- [x] Add TweenDestroyTargetOnStop
- [x] Journal
- [ ] Simplify documentation using AI

## NICE TO HAVE:

- [ ] Default TweenBuilder for use outside DOTs
- [ ] Separate Packages for extensions
  - [ ] Transform
  - [ ] UGUI Outputs
    - [ ] Rect Transform
    - [ ] Graphic
  - [ ] Material Outputs
  - [ ] AudioOneShotOnPlay/AudioOneShotOnStop
  - [ ] Timeline Outputs
  - [ ] Spline Outputs
- [ ] Graph Visualizer
- [ ] Dual-stage TweenStructuralChangeSystemGroup - one before and one after simulation to completely eliminate latency
- [ ] Scripting define clock switch

#### Other Projects:

- [ ] ECS Time Accuracy Test
- [ ] Create SourceGen library that walks the tree generating fragments, then later fragments combine the earlier fragments


