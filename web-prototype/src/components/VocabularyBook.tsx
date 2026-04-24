import React, { useState, useMemo } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { X, Search, Book, Volume2 } from 'lucide-react';

interface WordEntry {
  bn: string;
  phonetic: string;
  audio: string;
  category: string;
  difficulty: number;
}

interface VocabularyBookProps {
  show: boolean;
  onClose: () => void;
  collectedWords: string[];
  dictionary: Record<string, WordEntry>;
  onSpeak: (text: string) => void;
  totalWords: number;
}

export const VocabularyBook: React.FC<VocabularyBookProps> = ({ 
  show, 
  onClose, 
  collectedWords, 
  dictionary,
  onSpeak,
  totalWords
}) => {
  const [searchTerm, setSearchTerm] = useState('');

  const filteredWords = useMemo(() => {
    return collectedWords
      .filter(w => w.includes(searchTerm.toLowerCase()) || dictionary[w]?.bn.includes(searchTerm))
      .sort();
  }, [collectedWords, searchTerm, dictionary]);

  return (
    <AnimatePresence>
      {show && (
        <div className="fixed inset-0 z-[200] pointer-events-none">
          {/* Backdrop */}
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            onClick={onClose}
            className="absolute inset-0 bg-slate-950/80 backdrop-blur-sm pointer-events-auto"
          />

          {/* Drawer */}
          <motion.div
            initial={{ x: '100%' }}
            animate={{ x: 0 }}
            exit={{ x: '100%' }}
            transition={{ type: 'spring', damping: 25, stiffness: 200 }}
            className="absolute top-0 right-0 h-full w-full max-w-md bg-slate-900 border-l border-white/10 shadow-2xl pointer-events-auto flex flex-col"
          >
            {/* Header */}
            <div className="p-6 border-b border-white/10 flex items-center justify-between">
              <div>
                <h3 className="text-2xl font-black flex items-center gap-2">
                  <Book className="w-6 h-6 text-cyan-400" />
                  VOCAB BOOK
                </h3>
                <p className="text-xs text-slate-500 font-bold uppercase tracking-widest mt-1">
                  {collectedWords.length} / {totalWords} Words Found
                </p>
              </div>
              <button onClick={onClose} className="p-2 hover:bg-white/5 rounded-xl transition-colors">
                <X className="w-6 h-6" />
              </button>
            </div>

            {/* Search */}
            <div className="p-4">
              <div className="relative">
                <Search className="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-500" />
                <input 
                  type="text" 
                  placeholder="Search words or meanings..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="w-full bg-slate-950 border border-white/10 rounded-2xl py-3 pl-11 pr-4 text-sm font-medium focus:outline-none focus:border-cyan-500/50 transition-colors"
                />
              </div>
            </div>

            {/* List */}
            <div className="flex-1 overflow-y-auto p-4 custom-scrollbar">
              {filteredWords.length > 0 ? (
                <div className="grid gap-3">
                  {filteredWords.map((word) => {
                    const entry = dictionary[word];
                    return (
                      <motion.div 
                        key={word}
                        layout
                        className="glass p-4 rounded-2xl border-white/5 flex flex-col gap-2"
                      >
                        <div className="flex justify-between items-start">
                          <div>
                            <span className="text-xs font-black text-cyan-400 uppercase tracking-widest">{entry.category}</span>
                            <h4 className="text-xl font-black uppercase leading-tight">{word}</h4>
                            <p className="text-[10px] text-slate-500 font-bold italic">{entry.phonetic}</p>
                          </div>
                          <button 
                            onClick={() => onSpeak(word)}
                            className="p-3 bg-cyan-500/10 rounded-xl hover:bg-cyan-500/20 transition-all active:scale-90"
                          >
                            <Volume2 className="w-5 h-5 text-cyan-400" />
                          </button>
                        </div>
                        <div className="bg-slate-950/50 p-3 rounded-xl border border-white/5 mt-1">
                          <p className="text-2xl font-black text-cyan-300 font-bengali leading-none mb-1">
                            {entry.bn}
                          </p>
                        </div>
                      </motion.div>
                    );
                  })}
                </div>
              ) : (
                <div className="h-full flex flex-col items-center justify-center opacity-30 text-center px-10">
                  <Book className="w-16 h-16 mb-4" />
                  <p className="font-bold">No words found in your collection yet.</p>
                  <p className="text-xs mt-2 italic">Keep playing to discover new words!</p>
                </div>
              )}
            </div>
          </motion.div>
        </div>
      )}
    </AnimatePresence>
  );
};
