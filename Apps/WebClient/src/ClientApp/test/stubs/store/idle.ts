import {
    IdleActions,
    IdleGetters,
    IdleModule,
    IdleMutations,
    IdleState,
} from "@/store/modules/idle/types";

import { voidMethod } from "../util";

const idleState: IdleState = {
    isVisible: false,
};

const idleGetters: IdleGetters = {
    isVisible(): boolean {
        return false;
    },
};

const idleActions: IdleActions = {
    setVisibleState: voidMethod,
};

const idleMutations: IdleMutations = {
    setVisibleState: voidMethod,
};

const idleStub: IdleModule = {
    namespaced: true,
    state: idleState,
    getters: idleGetters,
    actions: idleActions,
    mutations: idleMutations,
};

export default idleStub;
