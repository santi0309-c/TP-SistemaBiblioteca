CC      = gcc
CFLAGS  = -Wall -Wextra -pedantic -std=c99 -Iinclude
TARGET  = microprocesador

SRCS    = src/main.c src/cpu.c src/decode.c src/execute.c
OBJS    = $(SRCS:.c=.o)

.PHONY: all clean run

all: $(TARGET)

$(TARGET): $(OBJS)
	$(CC) $(CFLAGS) -o $@ $^

src/%.o: src/%.c
	$(CC) $(CFLAGS) -c $< -o $@

run: all
	./$(TARGET)

clean:
	rm -f src/*.o $(TARGET)
