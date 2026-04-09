---
title: "Deployment Checklist"
description: "Pre-deployment verification steps"
order: 10
section: "Operations"
---

## Pre-Deployment

- [ ] All tests pass in CI
- [ ] Database migrations reviewed and tested
- [ ] Configuration changes documented
- [ ] Rollback plan prepared
- [ ] On-call engineer notified

## During Deployment

- [ ] Deploy to staging first
- [ ] Run smoke tests on staging
- [ ] Monitor error rates for 15 minutes
- [ ] Deploy to production
- [ ] Run production smoke tests

## Post-Deployment

- [ ] Verify key metrics are stable
- [ ] Update deployment log
- [ ] Close related tickets
